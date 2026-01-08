using System;

namespace RTCV.Plugins.MCPServer.MCP.Transport
{
    /// <summary>
    /// Base class for MCP transport implementations
    /// Provides common functionality for event raising and disposal
    /// </summary>
    public abstract class TransportBase : ITransport
    {
        private bool disposed;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<TransportErrorEventArgs> Error;

        public abstract bool IsConnected { get; }
        public abstract void Start();
        public abstract void Stop();
        public abstract void SendMessage(string message);

        /// <summary>
        /// Raise MessageReceived event
        /// </summary>
        protected void OnMessageReceived(string message)
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
        protected void OnError(string message, Exception exception = null)
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

        ~TransportBase()
        {
            Dispose(false);
        }
    }
}
