using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using RTCV.Common;
using RTCV.PluginHost;
using RTCV.Plugins.MCPServer.Config;
using RTCV.Plugins.MCPServer.Logging;
using RTCV.Plugins.MCPServer.MCP;
using RTCV.UI;

namespace RTCV.Plugins.MCPServer
{
    /// <summary>
    /// MCP Server Plugin - MEF Entry Point
    /// </summary>
    [Export(typeof(IPlugin))]
    public class PluginCore : IPlugin
    {
        public string Name => "MCP Server";
        public string Description => "Model Context Protocol server for RTCV - enables AI control via stdio";
        public string Author => "RTCV Community";
        public Version Version => new Version(1, 0, 0);
        public RTCSide SupportedSide => RTCSide.Server; // RTC side only

        private McpServer mcpServer;
        private ConfigManager configManager;
        private Logger logger;
        private bool started = false;

        public bool Start(RTCSide side)
        {
            try
            {
                if (started)
                {
                    return true;
                }

                // Only start on Server side
                if (side != RTCSide.Server && side != RTCSide.Both)
                {
                    return false;
                }

                // Initialize configuration
                configManager = new ConfigManager();
                ServerConfig config = configManager.LoadConfig();

                // Initialize logger
                logger = new Logger(config.Logging);
                logger.LogInfo("=== MCP Server Plugin Starting ===");
                logger.LogInfo($"Version: {Version}");

                // Register tool in RTCV UI
                try
                {
                    S.GET<OpenToolsForm>().RegisterTool(
                        "MCP Server",
                        "MCP Server Settings",
                        () => OpenSettingsForm()
                    );
                    logger.LogInfo("Registered MCP Server in Tools menu");
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to register tool in UI", ex);
                    // Don't fail plugin startup if UI registration fails
                }

                // Auto-start server if configured
                if (config.Server.AutoStart)
                {
                    try
                    {
                        mcpServer = new McpServer(config, logger);
                        mcpServer.Start();
                        logger.LogInfo("MCP Server auto-started on plugin load");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed to auto-start MCP server", ex);
                        MessageBox.Show(
                            $"MCP Server failed to auto-start:\n{ex.Message}\n\nYou can manually start it from Tools > MCP Server Settings",
                            "MCP Server",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                }

                started = true;
                logger.LogInfo("=== MCP Server Plugin Started Successfully ===");
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError("Fatal error starting MCP Server plugin", ex);
                MessageBox.Show(
                    $"Failed to start MCP Server plugin:\n{ex.Message}",
                    "MCP Server - Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        public bool StopPlugin()
        {
            try
            {
                logger?.LogInfo("=== MCP Server Plugin Stopping ===");

                // Stop MCP server if running
                if (mcpServer != null)
                {
                    try
                    {
                        mcpServer.Stop();
                        mcpServer.Dispose();
                        mcpServer = null;
                        logger?.LogInfo("MCP Server stopped");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError("Error stopping MCP server", ex);
                    }
                }

                // Save configuration
                if (configManager != null)
                {
                    try
                    {
                        // Config is saved automatically by the settings form
                        logger?.LogInfo("Configuration saved");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError("Error saving configuration", ex);
                    }
                }

                logger?.LogInfo("=== MCP Server Plugin Stopped ===");
                
                // Dispose logger
                logger?.Dispose();
                logger = null;

                started = false;
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError("Error stopping plugin", ex);
                return false;
            }
        }

        public void Dispose()
        {
            if (started)
            {
                StopPlugin();
            }
        }

        /// <summary>
        /// Open the MCP Server settings form
        /// </summary>
        private void OpenSettingsForm()
        {
            try
            {
                // Get or create the settings form as a singleton
                var settingsForm = S.GET<MCPServerForm>();
                
                // Initialize if not already initialized
                if (!settingsForm.Initialized)
                {
                    settingsForm.Initialize(this, configManager, logger);
                }

                // Show the form
                settingsForm.Show();
                settingsForm.Focus();
            }
            catch (Exception ex)
            {
                logger?.LogError("Error opening settings form", ex);
                MessageBox.Show(
                    $"Failed to open MCP Server settings:\n{ex.Message}",
                    "MCP Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Get the current MCP server instance (if running)
        /// </summary>
        public McpServer GetServer()
        {
            return mcpServer;
        }

        /// <summary>
        /// Start the MCP server
        /// </summary>
        public void StartServer()
        {
            if (mcpServer != null)
            {
                throw new InvalidOperationException("MCP Server is already running");
            }

            ServerConfig config = configManager.LoadConfig();
            mcpServer = new McpServer(config, logger);
            mcpServer.Start();
            logger.LogInfo("MCP Server started manually");
        }

        /// <summary>
        /// Stop the MCP server
        /// </summary>
        public void StopServer()
        {
            if (mcpServer == null)
            {
                throw new InvalidOperationException("MCP Server is not running");
            }

            mcpServer.Stop();
            mcpServer.Dispose();
            mcpServer = null;
            logger.LogInfo("MCP Server stopped manually");
        }

        /// <summary>
        /// Check if the MCP server is currently running
        /// </summary>
        public bool IsServerRunning()
        {
            return mcpServer != null && mcpServer.State == ServerState.Running;
        }
    }
}
