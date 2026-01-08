namespace RTCV.Plugins.MCPServer.Config
{
    /// <summary>
    /// Server transport and connection settings
    /// </summary>
    public class ServerSettings
    {
        /// <summary>
        /// Whether to automatically start the MCP server when the plugin loads
        /// </summary>
        public bool AutoStart { get; set; } = false;

        /// <summary>
        /// IP address to bind HTTP server to (future use)
        /// </summary>
        public string Address { get; set; } = "127.0.0.1";

        /// <summary>
        /// Port for HTTP server (future use)
        /// </summary>
        public int Port { get; set; } = 8080;

        /// <summary>
        /// Enable HTTP transport (not yet implemented)
        /// </summary>
        public bool EnableHttp { get; set; } = false;

        /// <summary>
        /// Enable stdio transport (default)
        /// </summary>
        public bool EnableStdio { get; set; } = true;
    }
}
