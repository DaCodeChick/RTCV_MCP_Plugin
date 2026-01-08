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
    public class StdioTransport : TransportBase
    {
        private Stream stdin;
        private Stream stdout;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread readThread;
        private CancellationTokenSource cancellation;
        private readonly TimeSpan shutdownTimeout;

        public override bool IsConnected => cancellation != null && !cancellation.IsCancellationRequested && stdin != null && stdout != null;

        /// <summary>
        /// Initialize stdio transport
        /// </summary>
        /// <param name="shutdownTimeoutMs">Graceful shutdown timeout in milliseconds (default: 2000ms)</param>
        public StdioTransport(int shutdownTimeoutMs = 2000)
        {
            this.shutdownTimeout = TimeSpan.FromMilliseconds(shutdownTimeoutMs);
        }

        /// <summary>
        /// Start the stdio transport
        /// </summary>
        public override void Start()
        {
            if (cancellation != null && !cancellation.IsCancellationRequested)
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

                cancellation = new CancellationTokenSource();

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
        public override void Stop()
        {
            if (cancellation == null || cancellation.IsCancellationRequested)
            {
                return;
            }

            try
            {
                // Signal cancellation
                cancellation.Cancel();

                // Wait for read thread to finish (with timeout)
                if (readThread != null && readThread.IsAlive)
                {
                    if (!readThread.Join(shutdownTimeout))
                    {
                        OnError("Read thread did not terminate gracefully within timeout");
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
            finally
            {
                cancellation?.Dispose();
                cancellation = null;
            }
        }

        /// <summary>
        /// Send a message to stdout
        /// </summary>
        /// <param name="message">JSON-RPC message to send</param>
        public override void SendMessage(string message)
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
                while (!cancellation.Token.IsCancellationRequested)
                {
                    // Read line from stdin (blocking)
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        // EOF reached - stdin closed
                        OnError("stdin closed (EOF)");
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
            catch (OperationCanceledException)
            {
                // Cancellation requested during shutdown - this is expected
            }
            catch (Exception ex)
            {
                OnError("Error in read loop", ex);
            }
        }
    }
}
