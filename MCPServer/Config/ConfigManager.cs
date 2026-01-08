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

            // Validate Server settings
            ValidateServerSettings(config.Server);

            // Ensure Logging settings exist
            if (config.Logging == null)
            {
                config.Logging = new Logging.LoggingSettings();
            }
        }

        /// <summary>
        /// Validate and fix server settings to ensure they are within acceptable ranges
        /// </summary>
        /// <param name="settings">Server settings to validate</param>
        private void ValidateServerSettings(ServerSettings settings)
        {
            bool hasWarnings = false;

            // Validate MaxRequestSizeBytes (must be > 0, warn if < 1KB or > 100MB)
            if (settings.MaxRequestSizeBytes <= 0)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] Invalid MaxRequestSizeBytes ({settings.MaxRequestSizeBytes}), using default (1MB)");
                settings.MaxRequestSizeBytes = 1024 * 1024;
                hasWarnings = true;
            }
            else if (settings.MaxRequestSizeBytes < 1024)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] MaxRequestSizeBytes is very small ({settings.MaxRequestSizeBytes} bytes), consider using at least 1KB");
                hasWarnings = true;
            }
            else if (settings.MaxRequestSizeBytes > 100 * 1024 * 1024)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] MaxRequestSizeBytes is very large ({settings.MaxRequestSizeBytes / (1024 * 1024)}MB), consider reducing to prevent DoS attacks");
                hasWarnings = true;
            }

            // Validate ShutdownTimeoutMs (must be >= 500, warn if < 1000 or > 30000)
            if (settings.ShutdownTimeoutMs < 500)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] Invalid ShutdownTimeoutMs ({settings.ShutdownTimeoutMs}), using minimum (500ms)");
                settings.ShutdownTimeoutMs = 500;
                hasWarnings = true;
            }
            else if (settings.ShutdownTimeoutMs > 30000)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] ShutdownTimeoutMs is very large ({settings.ShutdownTimeoutMs}ms), consider reducing for faster shutdown");
                hasWarnings = true;
            }

            // Validate MaxFileNameLength (must be >= 50, warn if < 100 or > 500)
            if (settings.MaxFileNameLength < 50)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] Invalid MaxFileNameLength ({settings.MaxFileNameLength}), using minimum (50)");
                settings.MaxFileNameLength = 50;
                hasWarnings = true;
            }
            else if (settings.MaxFileNameLength > 500)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] MaxFileNameLength is very large ({settings.MaxFileNameLength}), consider reducing");
                hasWarnings = true;
            }

            // Validate Port (must be 1-65535)
            if (settings.Port < 1 || settings.Port > 65535)
            {
                RTCVLogging.GlobalLogger.Warn($"[MCP Server] Invalid Port ({settings.Port}), using default (3000)");
                settings.Port = 3000;
                hasWarnings = true;
            }

            if (hasWarnings)
            {
                RTCVLogging.GlobalLogger.Info("[MCP Server] Configuration validation completed with warnings");
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
