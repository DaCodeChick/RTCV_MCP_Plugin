namespace RTCV.Plugins.MCPServer.Logging
{
    /// <summary>
    /// Logging verbosity levels
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Only log errors and critical events
        /// </summary>
        Minimal = 0,

        /// <summary>
        /// Log connection events, tool calls, and errors (default)
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Log all JSON-RPC messages and detailed execution traces
        /// </summary>
        Verbose = 2
    }

    /// <summary>
    /// Logging configuration settings
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Whether logging is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Path to log file (relative or absolute)
        /// </summary>
        public string Path { get; set; } = "Plugins/MCPServer/Logs/mcp.log";

        /// <summary>
        /// Logging verbosity level
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Normal;
    }
}
