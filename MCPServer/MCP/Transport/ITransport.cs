using System;

namespace RTCV.Plugins.MCPServer.MCP.Transport
{
    /// <summary>
    /// Event arguments for message received event
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Event arguments for transport error event
    /// </summary>
    public class TransportErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Message { get; }

        public TransportErrorEventArgs(string message, Exception exception = null)
        {
            Message = message;
            Exception = exception;
        }
    }

    /// <summary>
    /// Interface for MCP transport implementations
    /// </summary>
    public interface ITransport : IDisposable
    {
        /// <summary>
        /// Raised when a message is received
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a transport error occurs
        /// </summary>
        event EventHandler<TransportErrorEventArgs> Error;

        /// <summary>
        /// Start the transport (begin listening for messages)
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the transport (stop listening)
        /// </summary>
        void Stop();

        /// <summary>
        /// Send a message through the transport
        /// </summary>
        /// <param name="message">Message to send</param>
        void SendMessage(string message);

        /// <summary>
        /// Check if the transport is connected and ready
        /// </summary>
        bool IsConnected { get; }
    }
}
