using System;
using System.Windows.Forms;
using RTCV.Plugins.MCPServer.Config;
using RTCV.Plugins.MCPServer.Logging;
using RTCV.Plugins.MCPServer.MCP;
using RTCV.UI.Modular;

namespace RTCV.Plugins.MCPServer
{
    /// <summary>
    /// MCP Server Settings Form
    /// </summary>
    public partial class MCPServerForm : ComponentForm
    {
        private PluginCore plugin;
        private ConfigManager configManager;
        private Logger logger;

        public bool Initialized { get; private set; }

        public MCPServerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the form with plugin context
        /// </summary>
        public void Initialize(PluginCore plugin, ConfigManager configManager, Logger logger)
        {
            this.plugin = plugin;
            this.configManager = configManager;
            this.logger = logger;

            LoadSettings();
            UpdateServerStatus();

            Initialized = true;
        }

        /// <summary>
        /// Load settings from configuration
        /// </summary>
        private void LoadSettings()
        {
            if (configManager == null) return;

            ServerConfig config = configManager.LoadConfig();

            // Server settings
            chkAutoStart.Checked = config.Server.AutoStart;
            chkEnableStdio.Checked = config.Server.EnableStdio;
            chkEnableHttp.Checked = config.Server.EnableHttp;
            txtHttpAddress.Text = config.Server.Address;
            numHttpPort.Value = config.Server.Port;

            // Enable/disable HTTP controls based on checkbox
            txtHttpAddress.Enabled = config.Server.EnableHttp;
            numHttpPort.Enabled = config.Server.EnableHttp;

            // Logging settings
            chkLoggingEnabled.Checked = config.Logging.Enabled;
            cmbLogLevel.SelectedIndex = (int)config.Logging.Level;
            txtLogPath.Text = config.Logging.Path;

            // Tool settings
            chkMemoryReadEnabled.Checked = config.Tools.ContainsKey("memory_read") && config.Tools["memory_read"].Enabled;
            chkMemoryWriteEnabled.Checked = config.Tools.ContainsKey("memory_write") && config.Tools["memory_write"].Enabled;
        }

        /// <summary>
        /// Save settings to configuration
        /// </summary>
        private void SaveSettings()
        {
            if (configManager == null) return;

            ServerConfig config = configManager.LoadConfig();

            // Server settings
            config.Server.AutoStart = chkAutoStart.Checked;
            config.Server.EnableStdio = chkEnableStdio.Checked;
            config.Server.EnableHttp = chkEnableHttp.Checked;
            config.Server.Address = txtHttpAddress.Text;
            config.Server.Port = (int)numHttpPort.Value;

            // Logging settings
            config.Logging.Enabled = chkLoggingEnabled.Checked;
            config.Logging.Level = (LogLevel)cmbLogLevel.SelectedIndex;
            config.Logging.Path = txtLogPath.Text;

            // Tool settings
            if (config.Tools.ContainsKey("memory_read"))
            {
                config.Tools["memory_read"].Enabled = chkMemoryReadEnabled.Checked;
            }
            if (config.Tools.ContainsKey("memory_write"))
            {
                config.Tools["memory_write"].Enabled = chkMemoryWriteEnabled.Checked;
            }

            configManager.SaveConfig(config);
            logger?.LogInfo("Configuration saved from settings form");
        }

        /// <summary>
        /// Update the server status display
        /// </summary>
        private void UpdateServerStatus()
        {
            if (plugin == null) return;

            bool isRunning = plugin.IsServerRunning();
            
            lblServerStatus.Text = isRunning ? "Running" : "Stopped";
            lblServerStatus.ForeColor = isRunning ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            
            btnStart.Enabled = !isRunning;
            btnStop.Enabled = isRunning;
        }

        // Event Handlers

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // Save settings before starting
                SaveSettings();

                plugin.StartServer();
                UpdateServerStatus();
                logger?.LogInfo("MCP Server started from settings form");

                MessageBox.Show(
                    "MCP Server started successfully.\n\nThe server is now listening on stdio.",
                    "MCP Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                logger?.LogError("Error starting server from UI", ex);
                MessageBox.Show(
                    $"Failed to start MCP Server:\n{ex.Message}",
                    "MCP Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                plugin.StopServer();
                UpdateServerStatus();
                logger?.LogInfo("MCP Server stopped from settings form");

                MessageBox.Show(
                    "MCP Server stopped.",
                    "MCP Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                logger?.LogError("Error stopping server from UI", ex);
                MessageBox.Show(
                    $"Failed to stop MCP Server:\n{ex.Message}",
                    "MCP Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                MessageBox.Show(
                    "Settings saved successfully.",
                    "MCP Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                logger?.LogError("Error saving settings from UI", ex);
                MessageBox.Show(
                    $"Failed to save settings:\n{ex.Message}",
                    "MCP Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void chkEnableHttp_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable HTTP controls based on checkbox state
            txtHttpAddress.Enabled = chkEnableHttp.Checked;
            numHttpPort.Enabled = chkEnableHttp.Checked;
        }

        private void MCPServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide instead of close to maintain singleton
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
