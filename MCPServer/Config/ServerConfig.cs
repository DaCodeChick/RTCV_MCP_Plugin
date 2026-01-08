using System.Collections.Generic;
using RTCV.Plugins.MCPServer.Logging;

namespace RTCV.Plugins.MCPServer.Config
{
    /// <summary>
    /// Root configuration object for MCP Server
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// Configuration file format version
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Server transport and connection settings
        /// </summary>
        public ServerSettings Server { get; set; } = new ServerSettings();

        /// <summary>
        /// Logging configuration
        /// </summary>
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        /// <summary>
        /// Individual tool configuration (tool name -> settings)
        /// </summary>
        public Dictionary<string, ToolConfig> Tools { get; set; } = new Dictionary<string, ToolConfig>();

        /// <summary>
        /// Creates a default configuration with safe settings
        /// </summary>
        /// <returns>ServerConfig with all safe defaults</returns>
        public static ServerConfig CreateDefault()
        {
            var config = new ServerConfig();
            
            // Initialize all tools with safe defaults
            config.Tools["blast_generate"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["blast_toggle"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["blast_set_intensity"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["memory_domains_list"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["get_status"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["get_emulation_target_info"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["engine_get_config"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["engine_set_config"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["savestate_create"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["savestate_load"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["stockpile_add"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["stockpile_apply"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            
            // Dangerous tools disabled by default
            config.Tools["memory_read"] = new ToolConfig { Enabled = false, RequireConfirmation = true };
            config.Tools["memory_write"] = new ToolConfig { Enabled = false, RequireConfirmation = true };
            
            // Memory region annotation tools (safe, enabled by default)
            config.Tools["add_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["list_memory_regions"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["get_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["update_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["remove_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["read_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            config.Tools["write_memory_region"] = new ToolConfig { Enabled = true, RequireConfirmation = false };
            
            return config;
        }
    }
}
