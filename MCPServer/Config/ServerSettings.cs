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
        /// IP address or hostname to bind HTTP server to
        /// </summary>
        public string Address { get; set; } = "localhost";

        /// <summary>
        /// Port for HTTP server
        /// </summary>
        public int Port { get; set; } = 8080;

        /// <summary>
        /// Enable HTTP transport with Server-Sent Events (SSE)
        /// </summary>
        public bool EnableHttp { get; set; } = false;

        /// <summary>
        /// Enable stdio transport (default)
        /// </summary>
        public bool EnableStdio { get; set; } = true;
    }
}
