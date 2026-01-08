using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RTCVLogging = RTCV.Common.Logging;

namespace RTCV.Plugins.MCPServer.Config
{
    /// <summary>
    /// Manages loading and saving configuration to JSON file
    /// </summary>
    public class ConfigManager
    {
        private const string ConfigDirectory = "Plugins/MCPServer/Config";
        private const string ConfigFileName = "config.json";
        
        private readonly string configPath;

        public ConfigManager()
        {
            // Construct full path relative to RTCV installation
            configPath = Path.Combine(ConfigDirectory, ConfigFileName);
        }

        /// <summary>
        /// Load configuration from file, creating default if not found
        /// </summary>
        /// <returns>Loaded or default configuration</returns>
        public ServerConfig LoadConfig()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    RTCVLogging.GlobalLogger.Info($"[MCP Server] Created config directory: {directory}");
                }

                // Check if config file exists
                if (!File.Exists(configPath))
                {
                    RTCVLogging.GlobalLogger.Info($"[MCP Server] Config file not found, creating default: {configPath}");
                    var defaultConfig = ServerConfig.CreateDefault();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                // Read and deserialize
                string json = File.ReadAllText(configPath, Encoding.UTF8);
                var config = JsonConvert.DeserializeObject<ServerConfig>(json);
                
                if (config == null)
                {
                    RTCVLogging.GlobalLogger.Warn($"[MCP Server] Failed to deserialize config, using defaults");
                    return ServerConfig.CreateDefault();
                }

                // Validate and fill missing tools
                ValidateConfig(config);
                
                RTCVLogging.GlobalLogger.Info($"[MCP Server] Configuration loaded from: {configPath}");
                return config;
            }
            catch (Exception ex)
            {
                RTCVLogging.GlobalLogger.Error($"[MCP Server] Error loading config: {ex.Message}");
                RTCVLogging.GlobalLogger.Error(ex.ToString());
                RTCVLogging.GlobalLogger.Warn("[MCP Server] Using default configuration");
                return ServerConfig.CreateDefault();
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        /// <param name="config">Configuration to save</param>
        /// <returns>True if saved successfully</returns>
        public bool SaveConfig(ServerConfig config)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize with formatting
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };
                
                string json = JsonConvert.SerializeObject(config, settings);
                
                // Write to file with explicit UTF-8 encoding
                File.WriteAllText(configPath, json, Encoding.UTF8);
                
                RTCVLogging.GlobalLogger.Info($"[MCP Server] Configuration saved to: {configPath}");
                return true;
            }
            catch (Exception ex)
            {
                RTCVLogging.GlobalLogger.Error($"[MCP Server] Error saving config: {ex.Message}");
                RTCVLogging.GlobalLogger.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Validate configuration and add missing tools with defaults
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        private void ValidateConfig(ServerConfig config)
        {
            // Ensure Tools dictionary exists
            if (config.Tools == null)
            {
                config.Tools = new System.Collections.Generic.Dictionary<string, ToolConfig>();
            }

            // Get default config for reference
            var defaultConfig = ServerConfig.CreateDefault();

            // Add any missing tools from default
            foreach (var tool in defaultConfig.Tools)
            {
                if (!config.Tools.ContainsKey(tool.Key))
                {
                    config.Tools[tool.Key] = tool.Value;
                    RTCVLogging.GlobalLogger.Info($"[MCP Server] Added missing tool config: {tool.Key}");
                }
            }

            // Ensure Server settings exist
            if (config.Server == null)
            {
                config.Server = new ServerSettings();
            }

            // Ensure Logging settings exist
            if (config.Logging == null)
            {
                config.Logging = new Logging.LoggingSettings();
            }
        }

        /// <summary>
        /// Get the full path to the configuration file
        /// </summary>
        public string GetConfigPath()
        {
            return Path.GetFullPath(configPath);
        }
    }
}
