namespace RTCV.Plugins.MCPServer.MCP
{
    /// <summary>
    /// MCP Server state
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// Server is stopped and not running
        /// </summary>
        Stopped,

        /// <summary>
        /// Server is in the process of starting up
        /// </summary>
        Starting,

        /// <summary>
        /// Server is running and accepting requests
        /// </summary>
        Running,

        /// <summary>
        /// Server is in the process of shutting down
        /// </summary>
        Stopping
    }
}
