namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// Simple static logger helper for tool handlers
    /// </summary>
    internal static class ToolLogger
    {
        public static void Log(string message)
        {
            RTCV.Common.Logging.GlobalLogger.Info($"[MCP Tool] {message}");
        }

        public static void LogError(string message)
        {
            RTCV.Common.Logging.GlobalLogger.Error($"[MCP Tool] {message}");
        }
    }
}
