using System;
using System.IO;
using System.Threading;

namespace RTCV.Plugins.MCPServer.MCP.Transport
{
    /// <summary>
    /// stdio transport for MCP (reads from stdin, writes to stdout)
    /// CRITICAL: Only JSON-RPC messages should be written to stdout
    /// All logging must go to file or stderr
    /// </summary>
    public class StdioTransport : ITransport
    {
        private Stream stdin;
        private Stream stdout;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread readThread;
        private bool isRunning;
        private bool disposed;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<TransportErrorEventArgs> Error;

        public bool IsConnected => isRunning && stdin != null && stdout != null;

        /// <summary>
        /// Start the stdio transport
        /// </summary>
        public void Start()
        {
            if (isRunning)
            {
                throw new InvalidOperationException("Transport is already running");
            }

            try
            {
                // Open standard streams
                stdin = Console.OpenStandardInput();
                stdout = Console.OpenStandardOutput();

                // Create readers/writers
                reader = new StreamReader(stdin);
                writer = new StreamWriter(stdout)
                {
                    AutoFlush = true,
                    NewLine = "\n" // Use Unix line endings for consistency
                };

                isRunning = true;

                // Start background thread for reading
                readThread = new Thread(ReadLoop)
                {
                    Name = "MCP-StdioTransport-Reader",
                    IsBackground = true
                };
                readThread.Start();

                OnError("stdio transport started");
            }
            catch (Exception ex)
            {
                OnError("Failed to start stdio transport", ex);
                throw;
            }
        }

        /// <summary>
        /// Stop the stdio transport
        /// </summary>
        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;

            try
            {
                // Wait for read thread to finish (with timeout)
                if (readThread != null && readThread.IsAlive)
                {
                    if (!readThread.Join(TimeSpan.FromSeconds(2)))
                    {
                        // Force abort if it doesn't finish
                        readThread.Abort();
                    }
                }

                // Close streams
                reader?.Dispose();
                writer?.Dispose();
                
                // Note: We don't close stdin/stdout themselves as they're owned by the process

                OnError("stdio transport stopped");
            }
            catch (Exception ex)
            {
                OnError("Error stopping stdio transport", ex);
            }
        }

        /// <summary>
        /// Send a message to stdout
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
                lock (writer)
                {
                    // Write message followed by newline
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                OnError("Failed to send message", ex);
                throw;
            }
        }

        /// <summary>
        /// Background thread for reading from stdin
        /// </summary>
        private void ReadLoop()
        {
            try
            {
                while (isRunning)
                {
                    // Read line from stdin (blocking)
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        // EOF reached - stdin closed
                        OnError("stdin closed (EOF)");
                        isRunning = false;
                        break;
                    }

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // Raise MessageReceived event
                    OnMessageReceived(line);
                }
            }
            catch (ThreadAbortException)
            {
                // Thread was aborted during shutdown - this is expected
            }
            catch (Exception ex)
            {
                OnError("Error in read loop", ex);
                isRunning = false;
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
        /// NOTE: This is for transport-level errors, not logging
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
                }
                disposed = true;
            }
        }

        ~StdioTransport()
        {
            Dispose(false);
        }
    }
}
