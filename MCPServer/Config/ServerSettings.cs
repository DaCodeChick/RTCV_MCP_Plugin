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

        /// <summary>
        /// Maximum request size in bytes for HTTP transport (protects against DoS attacks)
        /// Default: 1048576 (1MB)
        /// </summary>
        public int MaxRequestSizeBytes { get; set; } = 1024 * 1024;

        /// <summary>
        /// Timeout in milliseconds for graceful thread shutdown during transport stop
        /// Default: 2000 (2 seconds)
        /// </summary>
        public int ShutdownTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// Maximum filename length for emulation target display (prevents UI overflow)
        /// Default: 200 characters
        /// </summary>
        public int MaxFileNameLength { get; set; } = 200;
    }
}
