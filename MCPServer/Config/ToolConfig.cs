namespace RTCV.Plugins.MCPServer.Config
{
    /// <summary>
    /// Individual tool configuration settings
    /// </summary>
    public class ToolConfig
    {
        /// <summary>
        /// Whether this tool is enabled and can be called
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether this tool requires user confirmation before execution
        /// </summary>
        public bool RequireConfirmation { get; set; } = false;
    }
}
