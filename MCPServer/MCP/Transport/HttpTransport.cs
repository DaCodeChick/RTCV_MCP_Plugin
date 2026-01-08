using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace RTCV.Plugins.MCPServer.MCP.Transport
{
    /// <summary>
    /// HTTP transport for MCP using Server-Sent Events (SSE)
    /// - POST /message: Client sends JSON-RPC requests
    /// - GET /sse: Server sends responses via Server-Sent Events
    /// </summary>
    public class HttpTransport : ITransport
    {
        private HttpListener listener;
        private Thread listenerThread;
        private Thread sseWriterThread;
        private CancellationTokenSource cancellationTokenSource;
        private bool disposed;
        
        private readonly string host;
        private readonly int port;
        private readonly string prefix;
        private readonly int maxRequestSizeBytes;
        private readonly TimeSpan shutdownTimeout;
        
        // SSE connection management
        private HttpListenerResponse sseResponse;
        private StreamWriter sseWriter;
        private readonly object sseLock = new object();
        
        // Message queue for SSE
        private readonly BlockingCollection<string> messageQueue = new BlockingCollection<string>();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<TransportErrorEventArgs> Error;

        public bool IsConnected 
        { 
            get 
            { 
                return cancellationTokenSource != null && 
                       !cancellationTokenSource.IsCancellationRequested && 
                       listener != null && 
                       listener.IsListening; 
            } 
        }

        /// <summary>
        /// Initialize HTTP transport
        /// </summary>
        /// <param name="host">Host to bind to (default: localhost)</param>
        /// <param name="port">Port to listen on (default: 8080)</param>
        /// <param name="maxRequestSizeBytes">Maximum request size in bytes (default: 1MB)</param>
        /// <param name="shutdownTimeoutMs">Graceful shutdown timeout in milliseconds (default: 2000ms)</param>
        public HttpTransport(string host = "localhost", int port = 8080, int maxRequestSizeBytes = 1024 * 1024, int shutdownTimeoutMs = 2000)
        {
            this.host = host;
            this.port = port;
            this.prefix = $"http://{host}:{port}/";
            this.maxRequestSizeBytes = maxRequestSizeBytes;
            this.shutdownTimeout = TimeSpan.FromMilliseconds(shutdownTimeoutMs);
        }

        /// <summary>
        /// Start the HTTP transport
        /// </summary>
        public void Start()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("Transport is already running");
            }

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                
                listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                listener.Start();

                // Start listener thread
                listenerThread = new Thread(ListenerLoop)
                {
                    Name = "MCP-HttpTransport-Listener",
                    IsBackground = true
                };
                listenerThread.Start();

                // Start SSE writer thread
                sseWriterThread = new Thread(SseWriterLoop)
                {
                    Name = "MCP-HttpTransport-SSEWriter",
                    IsBackground = true
                };
                sseWriterThread.Start();

                OnError($"HTTP transport started on {prefix}");
            }
            catch (Exception ex)
            {
                OnError("Failed to start HTTP transport", ex);
                throw;
            }
        }

        /// <summary>
        /// Stop the HTTP transport
        /// </summary>
        public void Stop()
        {
            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            try
            {
                // Signal cancellation
                cancellationTokenSource.Cancel();

                // Stop accepting new connections
                listener?.Stop();

                // Close SSE connection
                lock (sseLock)
                {
                    try
                    {
                        sseWriter?.Close();
                    }
                    catch { }
                    sseWriter = null;
                    sseResponse = null;
                }

                // Stop message queue
                messageQueue.CompleteAdding();

                // Wait for threads to finish gracefully
                if (listenerThread != null && listenerThread.IsAlive)
                {
                    listenerThread.Join(shutdownTimeout);
                }

                if (sseWriterThread != null && sseWriterThread.IsAlive)
                {
                    sseWriterThread.Join(shutdownTimeout);
                }

                listener?.Close();
                OnError("HTTP transport stopped");
            }
            catch (Exception ex)
            {
                OnError("Error stopping HTTP transport", ex);
            }
        }

        /// <summary>
        /// Send a message via SSE
        /// </summary>
        /// <param name="message">JSON-RPC message to send</param>
        public void SendMessage(string message)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Transport is not connected");
            }

            try
            {
                // Add message to queue for SSE writer thread
                messageQueue.Add(message);
            }
            catch (Exception ex)
            {
                OnError("Failed to queue message for SSE", ex);
                throw;
            }
        }

        /// <summary>
        /// Main listener loop - handles incoming HTTP requests
        /// </summary>
        private void ListenerLoop()
        {
            var token = cancellationTokenSource.Token;
            
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Wait for incoming request (blocking)
                        HttpListenerContext context = listener.GetContext();
                        
                        // Handle request on a separate thread to avoid blocking
                        ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                    }
                    catch (HttpListenerException)
                    {
                        // Listener was stopped - exit gracefully
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        // Otherwise, log and continue
                    }
                    catch (ObjectDisposedException)
                    {
                        // Listener was disposed - exit gracefully
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    OnError("Error in listener loop", ex);
                }
            }
        }

        /// <summary>
        /// Handle an incoming HTTP request
        /// </summary>
        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Add CORS headers
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                // Handle OPTIONS preflight
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // GET /sse - Server-Sent Events endpoint
                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/sse")
                {
                    HandleSseConnection(context);
                    return;
                }

                // POST /message - JSON-RPC endpoint
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/message")
                {
                    HandleMessage(context);
                    return;
                }

                // Unknown endpoint
                response.StatusCode = 404;
                byte[] errorBytes = Encoding.UTF8.GetBytes("Not Found");
                response.ContentLength64 = errorBytes.Length;
                response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                OnError("Error handling HTTP request", ex);
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// Handle SSE connection establishment
        /// </summary>
        private void HandleSseConnection(HttpListenerContext context)
        {
            lock (sseLock)
            {
                // Close existing SSE connection if any
                if (sseWriter != null)
                {
                    try
                    {
                        sseWriter.Close();
                    }
                    catch { }
                }

                // Setup new SSE connection
                HttpListenerResponse response = context.Response;
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                
                // Keep the response stream open for SSE
                sseResponse = response;
                sseWriter = new StreamWriter(response.OutputStream, Encoding.UTF8)
                {
                    AutoFlush = true,
                    NewLine = "\n"
                };

                OnError("SSE client connected");

                // Send initial connection message
                try
                {
                    sseWriter.WriteLine("event: endpoint");
                    sseWriter.WriteLine($"data: {prefix}message");
                    sseWriter.WriteLine();
                }
                catch (Exception ex)
                {
                    OnError("Failed to send SSE endpoint message", ex);
                }
            }
        }

        /// <summary>
        /// Handle incoming JSON-RPC message
        /// </summary>
        private void HandleMessage(HttpListenerContext context)
        {
            try
            {
                // Check request size to prevent DoS attacks
                if (context.Request.ContentLength64 > maxRequestSizeBytes)
                {
                    context.Response.StatusCode = 413; // Payload Too Large
                    byte[] errorBytes = Encoding.UTF8.GetBytes($"Request too large. Maximum size is {maxRequestSizeBytes} bytes.");
                    context.Response.ContentLength64 = errorBytes.Length;
                    context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                    context.Response.Close();
                    OnError($"Rejected request exceeding size limit: {context.Request.ContentLength64} bytes");
                    return;
                }

                // Read request body
                using (StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    string message = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        context.Response.StatusCode = 400;
                        byte[] errorBytes = Encoding.UTF8.GetBytes("Empty request body");
                        context.Response.ContentLength64 = errorBytes.Length;
                        context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                        context.Response.Close();
                        return;
                    }

                    // Raise MessageReceived event
                    OnMessageReceived(message);

                    // Send 202 Accepted response (response will come via SSE)
                    context.Response.StatusCode = 202;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                OnError("Error handling message", ex);
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        /// <summary>
        /// SSE writer loop - sends messages from queue via SSE
        /// </summary>
        private void SseWriterLoop()
        {
            var token = cancellationTokenSource.Token;
            
            try
            {
                foreach (var message in messageQueue.GetConsumingEnumerable(token))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    lock (sseLock)
                    {
                        if (sseWriter == null)
                        {
                            // No SSE client connected, skip message
                            continue;
                        }

                        try
                        {
                            // Send message as SSE event
                            sseWriter.WriteLine("event: message");
                            sseWriter.WriteLine($"data: {message}");
                            sseWriter.WriteLine();
                        }
                        catch (Exception ex)
                        {
                            OnError("Failed to send SSE message", ex);
                            
                            // Close broken connection
                            try
                            {
                                sseWriter.Close();
                            }
                            catch { }
                            sseWriter = null;
                            sseResponse = null;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled - this is expected during shutdown
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    OnError("Error in SSE writer loop", ex);
                }
            }
        }

        /// <summary>
        /// Raise MessageReceived event
        /// </summary>
        private void OnMessageReceived(string message)
        {
            try
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            }
            catch (Exception ex)
            {
                OnError("Error in MessageReceived handler", ex);
            }
        }

        /// <summary>
        /// Raise Error event
        /// </summary>
        private void OnError(string message, Exception exception = null)
        {
            try
            {
                Error?.Invoke(this, new TransportErrorEventArgs(message, exception));
            }
            catch
            {
                // Silently fail - we can't do anything if error handler fails
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                    cancellationTokenSource?.Dispose();
                    messageQueue?.Dispose();
                }
                disposed = true;
            }
        }

        ~HttpTransport()
        {
            Dispose(false);
        }
    }
}
