namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using RTCV.Common.Logging;

    /// <summary>
    /// Simple static logger helper for tool handlers
    /// </summary>
    internal static class ToolLogger
    {
        public static void Log(string message)
        {
            GlobalLogger.Info($"[MCP Tool] {message}");
        }

        public static void LogError(string message)
        {
            GlobalLogger.Error($"[MCP Tool] {message}");
        }
    }
}
