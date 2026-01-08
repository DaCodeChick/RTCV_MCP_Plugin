namespace RTCV.Plugins.MCPServer
{
    partial class MCPServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpServerControl = new System.Windows.Forms.GroupBox();
            this.lblServerStatusLabel = new System.Windows.Forms.Label();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.grpServerSettings = new System.Windows.Forms.GroupBox();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkEnableStdio = new System.Windows.Forms.CheckBox();
            this.chkEnableHttp = new System.Windows.Forms.CheckBox();
            this.lblHttpAddress = new System.Windows.Forms.Label();
            this.txtHttpAddress = new System.Windows.Forms.TextBox();
            this.lblHttpPort = new System.Windows.Forms.Label();
            this.numHttpPort = new System.Windows.Forms.NumericUpDown();
            this.grpLogging = new System.Windows.Forms.GroupBox();
            this.chkLoggingEnabled = new System.Windows.Forms.CheckBox();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.grpToolSettings = new System.Windows.Forms.GroupBox();
            this.chkMemoryReadEnabled = new System.Windows.Forms.CheckBox();
            this.chkMemoryWriteEnabled = new System.Windows.Forms.CheckBox();
            this.lblMemoryWarning = new System.Windows.Forms.Label();
            this.grpAdvancedSettings = new System.Windows.Forms.GroupBox();
            this.lblMaxRequestSize = new System.Windows.Forms.Label();
            this.numMaxRequestSize = new System.Windows.Forms.NumericUpDown();
            this.lblMaxRequestSizeMB = new System.Windows.Forms.Label();
            this.lblShutdownTimeout = new System.Windows.Forms.Label();
            this.numShutdownTimeout = new System.Windows.Forms.NumericUpDown();
            this.lblShutdownTimeoutMs = new System.Windows.Forms.Label();
            this.lblMaxFilenameLength = new System.Windows.Forms.Label();
            this.numMaxFilenameLength = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpServerControl.SuspendLayout();
            this.grpServerSettings.SuspendLayout();
            this.grpLogging.SuspendLayout();
            this.grpToolSettings.SuspendLayout();
            this.grpAdvancedSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRequestSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numShutdownTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFilenameLength)).BeginInit();
            this.SuspendLayout();
            // 
            // grpServerControl
            // 
            this.grpServerControl.Controls.Add(this.lblServerStatusLabel);
            this.grpServerControl.Controls.Add(this.lblServerStatus);
            this.grpServerControl.Controls.Add(this.btnStart);
            this.grpServerControl.Controls.Add(this.btnStop);
            this.grpServerControl.Location = new System.Drawing.Point(12, 12);
            this.grpServerControl.Name = "grpServerControl";
            this.grpServerControl.Size = new System.Drawing.Size(460, 80);
            this.grpServerControl.TabIndex = 0;
            this.grpServerControl.TabStop = false;
            this.grpServerControl.Text = "Server Control";
            // 
            // lblServerStatusLabel
            // 
            this.lblServerStatusLabel.AutoSize = true;
            this.lblServerStatusLabel.Location = new System.Drawing.Point(15, 25);
            this.lblServerStatusLabel.Name = "lblServerStatusLabel";
            this.lblServerStatusLabel.Size = new System.Drawing.Size(40, 13);
            this.lblServerStatusLabel.TabIndex = 0;
            this.lblServerStatusLabel.Text = "Status:";
            // 
            // lblServerStatus
            // 
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblServerStatus.Location = new System.Drawing.Point(61, 25);
            this.lblServerStatus.Name = "lblServerStatus";
            this.lblServerStatus.Size = new System.Drawing.Size(53, 13);
            this.lblServerStatus.TabIndex = 1;
            this.lblServerStatus.Text = "Stopped";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(15, 45);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(121, 45);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 23);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop Server";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // grpServerSettings
            // 
            this.grpServerSettings.Controls.Add(this.numHttpPort);
            this.grpServerSettings.Controls.Add(this.lblHttpPort);
            this.grpServerSettings.Controls.Add(this.txtHttpAddress);
            this.grpServerSettings.Controls.Add(this.lblHttpAddress);
            this.grpServerSettings.Controls.Add(this.chkEnableHttp);
            this.grpServerSettings.Controls.Add(this.chkAutoStart);
            this.grpServerSettings.Controls.Add(this.chkEnableStdio);
            this.grpServerSettings.Location = new System.Drawing.Point(12, 98);
            this.grpServerSettings.Name = "grpServerSettings";
            this.grpServerSettings.Size = new System.Drawing.Size(460, 150);
            this.grpServerSettings.TabIndex = 1;
            this.grpServerSettings.TabStop = false;
            this.grpServerSettings.Text = "Server Settings";
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(15, 25);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(178, 17);
            this.chkAutoStart.TabIndex = 0;
            this.chkAutoStart.Text = "Auto-start server when RTCV loads";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // chkEnableStdio
            // 
            this.chkEnableStdio.AutoSize = true;
            this.chkEnableStdio.Checked = true;
            this.chkEnableStdio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableStdio.Location = new System.Drawing.Point(15, 48);
            this.chkEnableStdio.Name = "chkEnableStdio";
            this.chkEnableStdio.Size = new System.Drawing.Size(120, 17);
            this.chkEnableStdio.TabIndex = 1;
            this.chkEnableStdio.Text = "Enable stdio transport";
            this.chkEnableStdio.UseVisualStyleBackColor = true;
            // 
            // chkEnableHttp
            // 
            this.chkEnableHttp.AutoSize = true;
            this.chkEnableHttp.Location = new System.Drawing.Point(15, 71);
            this.chkEnableHttp.Name = "chkEnableHttp";
            this.chkEnableHttp.Size = new System.Drawing.Size(120, 17);
            this.chkEnableHttp.TabIndex = 2;
            this.chkEnableHttp.Text = "Enable HTTP transport";
            this.chkEnableHttp.UseVisualStyleBackColor = true;
            this.chkEnableHttp.CheckedChanged += new System.EventHandler(this.chkEnableHttp_CheckedChanged);
            // 
            // lblHttpAddress
            // 
            this.lblHttpAddress.AutoSize = true;
            this.lblHttpAddress.Location = new System.Drawing.Point(35, 98);
            this.lblHttpAddress.Name = "lblHttpAddress";
            this.lblHttpAddress.Size = new System.Drawing.Size(48, 13);
            this.lblHttpAddress.TabIndex = 3;
            this.lblHttpAddress.Text = "Address:";
            // 
            // txtHttpAddress
            // 
            this.txtHttpAddress.Enabled = false;
            this.txtHttpAddress.Location = new System.Drawing.Point(89, 95);
            this.txtHttpAddress.Name = "txtHttpAddress";
            this.txtHttpAddress.Size = new System.Drawing.Size(150, 20);
            this.txtHttpAddress.TabIndex = 4;
            this.txtHttpAddress.Text = "localhost";
            // 
            // lblHttpPort
            // 
            this.lblHttpPort.AutoSize = true;
            this.lblHttpPort.Location = new System.Drawing.Point(260, 98);
            this.lblHttpPort.Name = "lblHttpPort";
            this.lblHttpPort.Size = new System.Drawing.Size(29, 13);
            this.lblHttpPort.TabIndex = 5;
            this.lblHttpPort.Text = "Port:";
            // 
            // numHttpPort
            // 
            this.numHttpPort.Enabled = false;
            this.numHttpPort.Location = new System.Drawing.Point(295, 96);
            this.numHttpPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numHttpPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
            this.numHttpPort.Name = "numHttpPort";
            this.numHttpPort.Size = new System.Drawing.Size(80, 20);
            this.numHttpPort.TabIndex = 6;
            this.numHttpPort.Value = new decimal(new int[] { 8080, 0, 0, 0 });
            // 
            // grpLogging
            // 
            this.grpLogging.Controls.Add(this.chkLoggingEnabled);
            this.grpLogging.Controls.Add(this.lblLogLevel);
            this.grpLogging.Controls.Add(this.cmbLogLevel);
            this.grpLogging.Controls.Add(this.lblLogPath);
            this.grpLogging.Controls.Add(this.txtLogPath);
            this.grpLogging.Location = new System.Drawing.Point(12, 254);
            this.grpLogging.Name = "grpLogging";
            this.grpLogging.Size = new System.Drawing.Size(460, 105);
            this.grpLogging.TabIndex = 2;
            this.grpLogging.TabStop = false;
            this.grpLogging.Text = "Logging";
            // 
            // chkLoggingEnabled
            // 
            this.chkLoggingEnabled.AutoSize = true;
            this.chkLoggingEnabled.Checked = true;
            this.chkLoggingEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLoggingEnabled.Location = new System.Drawing.Point(15, 25);
            this.chkLoggingEnabled.Name = "chkLoggingEnabled";
            this.chkLoggingEnabled.Size = new System.Drawing.Size(105, 17);
            this.chkLoggingEnabled.TabIndex = 0;
            this.chkLoggingEnabled.Text = "Enable logging";
            this.chkLoggingEnabled.UseVisualStyleBackColor = true;
            // 
            // lblLogLevel
            // 
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Location = new System.Drawing.Point(15, 48);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(56, 13);
            this.lblLogLevel.TabIndex = 1;
            this.lblLogLevel.Text = "Log Level:";
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] {
            "Minimal",
            "Normal",
            "Verbose"});
            this.cmbLogLevel.Location = new System.Drawing.Point(77, 45);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(150, 21);
            this.cmbLogLevel.TabIndex = 2;
            // 
            // lblLogPath
            // 
            this.lblLogPath.AutoSize = true;
            this.lblLogPath.Location = new System.Drawing.Point(15, 75);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(53, 13);
            this.lblLogPath.TabIndex = 3;
            this.lblLogPath.Text = "Log Path:";
            // 
            // txtLogPath
            // 
            this.txtLogPath.Location = new System.Drawing.Point(77, 72);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(370, 20);
            this.txtLogPath.TabIndex = 4;
            // 
            // grpToolSettings
            // 
            this.grpToolSettings.Controls.Add(this.chkMemoryReadEnabled);
            this.grpToolSettings.Controls.Add(this.chkMemoryWriteEnabled);
            this.grpToolSettings.Controls.Add(this.lblMemoryWarning);
            this.grpToolSettings.Location = new System.Drawing.Point(12, 365);
            this.grpToolSettings.Name = "grpToolSettings";
            this.grpToolSettings.Size = new System.Drawing.Size(460, 100);
            this.grpToolSettings.TabIndex = 3;
            this.grpToolSettings.TabStop = false;
            this.grpToolSettings.Text = "Tool Settings";
            // 
            // chkMemoryReadEnabled
            // 
            this.chkMemoryReadEnabled.AutoSize = true;
            this.chkMemoryReadEnabled.Location = new System.Drawing.Point(15, 25);
            this.chkMemoryReadEnabled.Name = "chkMemoryReadEnabled";
            this.chkMemoryReadEnabled.Size = new System.Drawing.Size(156, 17);
            this.chkMemoryReadEnabled.TabIndex = 0;
            this.chkMemoryReadEnabled.Text = "Enable memory_read tool";
            this.chkMemoryReadEnabled.UseVisualStyleBackColor = true;
            // 
            // chkMemoryWriteEnabled
            // 
            this.chkMemoryWriteEnabled.AutoSize = true;
            this.chkMemoryWriteEnabled.Location = new System.Drawing.Point(15, 48);
            this.chkMemoryWriteEnabled.Name = "chkMemoryWriteEnabled";
            this.chkMemoryWriteEnabled.Size = new System.Drawing.Size(157, 17);
            this.chkMemoryWriteEnabled.TabIndex = 1;
            this.chkMemoryWriteEnabled.Text = "Enable memory_write tool";
            this.chkMemoryWriteEnabled.UseVisualStyleBackColor = true;
            // 
            // lblMemoryWarning
            // 
            this.lblMemoryWarning.ForeColor = System.Drawing.Color.Red;
            this.lblMemoryWarning.Location = new System.Drawing.Point(15, 70);
            this.lblMemoryWarning.Name = "lblMemoryWarning";
            this.lblMemoryWarning.Size = new System.Drawing.Size(430, 20);
            this.lblMemoryWarning.TabIndex = 2;
            this.lblMemoryWarning.Text = "⚠ Warning: Memory tools allow direct memory access. Enable at your own risk.";
            // 
            // grpAdvancedSettings
            // 
            this.grpAdvancedSettings.Controls.Add(this.lblMaxRequestSize);
            this.grpAdvancedSettings.Controls.Add(this.numMaxRequestSize);
            this.grpAdvancedSettings.Controls.Add(this.lblMaxRequestSizeMB);
            this.grpAdvancedSettings.Controls.Add(this.lblShutdownTimeout);
            this.grpAdvancedSettings.Controls.Add(this.numShutdownTimeout);
            this.grpAdvancedSettings.Controls.Add(this.lblShutdownTimeoutMs);
            this.grpAdvancedSettings.Controls.Add(this.lblMaxFilenameLength);
            this.grpAdvancedSettings.Controls.Add(this.numMaxFilenameLength);
            this.grpAdvancedSettings.Location = new System.Drawing.Point(12, 254);
            this.grpAdvancedSettings.Name = "grpAdvancedSettings";
            this.grpAdvancedSettings.Size = new System.Drawing.Size(460, 105);
            this.grpAdvancedSettings.TabIndex = 6;
            this.grpAdvancedSettings.TabStop = false;
            this.grpAdvancedSettings.Text = "Advanced Settings";
            // 
            // lblMaxRequestSize
            // 
            this.lblMaxRequestSize.AutoSize = true;
            this.lblMaxRequestSize.Location = new System.Drawing.Point(15, 25);
            this.lblMaxRequestSize.Name = "lblMaxRequestSize";
            this.lblMaxRequestSize.Size = new System.Drawing.Size(130, 13);
            this.lblMaxRequestSize.TabIndex = 0;
            this.lblMaxRequestSize.Text = "Max HTTP Request Size:";
            // 
            // numMaxRequestSize
            // 
            this.numMaxRequestSize.Location = new System.Drawing.Point(151, 23);
            this.numMaxRequestSize.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numMaxRequestSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxRequestSize.Name = "numMaxRequestSize";
            this.numMaxRequestSize.Size = new System.Drawing.Size(70, 20);
            this.numMaxRequestSize.TabIndex = 1;
            this.numMaxRequestSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblMaxRequestSizeMB
            // 
            this.lblMaxRequestSizeMB.AutoSize = true;
            this.lblMaxRequestSizeMB.Location = new System.Drawing.Point(227, 25);
            this.lblMaxRequestSizeMB.Name = "lblMaxRequestSizeMB";
            this.lblMaxRequestSizeMB.Size = new System.Drawing.Size(23, 13);
            this.lblMaxRequestSizeMB.TabIndex = 2;
            this.lblMaxRequestSizeMB.Text = "MB";
            // 
            // lblShutdownTimeout
            // 
            this.lblShutdownTimeout.AutoSize = true;
            this.lblShutdownTimeout.Location = new System.Drawing.Point(15, 51);
            this.lblShutdownTimeout.Name = "lblShutdownTimeout";
            this.lblShutdownTimeout.Size = new System.Drawing.Size(104, 13);
            this.lblShutdownTimeout.TabIndex = 3;
            this.lblShutdownTimeout.Text = "Shutdown Timeout:";
            // 
            // numShutdownTimeout
            // 
            this.numShutdownTimeout.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.numShutdownTimeout.Location = new System.Drawing.Point(151, 49);
            this.numShutdownTimeout.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numShutdownTimeout.Minimum = new decimal(new int[] { 500, 0, 0, 0 });
            this.numShutdownTimeout.Name = "numShutdownTimeout";
            this.numShutdownTimeout.Size = new System.Drawing.Size(70, 20);
            this.numShutdownTimeout.TabIndex = 4;
            this.numShutdownTimeout.Value = new decimal(new int[] { 2000, 0, 0, 0 });
            // 
            // lblShutdownTimeoutMs
            // 
            this.lblShutdownTimeoutMs.AutoSize = true;
            this.lblShutdownTimeoutMs.Location = new System.Drawing.Point(227, 51);
            this.lblShutdownTimeoutMs.Name = "lblShutdownTimeoutMs";
            this.lblShutdownTimeoutMs.Size = new System.Drawing.Size(20, 13);
            this.lblShutdownTimeoutMs.TabIndex = 5;
            this.lblShutdownTimeoutMs.Text = "ms";
            // 
            // lblMaxFilenameLength
            // 
            this.lblMaxFilenameLength.AutoSize = true;
            this.lblMaxFilenameLength.Location = new System.Drawing.Point(15, 77);
            this.lblMaxFilenameLength.Name = "lblMaxFilenameLength";
            this.lblMaxFilenameLength.Size = new System.Drawing.Size(116, 13);
            this.lblMaxFilenameLength.TabIndex = 6;
            this.lblMaxFilenameLength.Text = "Max Filename Length:";
            // 
            // numMaxFilenameLength
            // 
            this.numMaxFilenameLength.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            this.numMaxFilenameLength.Location = new System.Drawing.Point(151, 75);
            this.numMaxFilenameLength.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.numMaxFilenameLength.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            this.numMaxFilenameLength.Name = "numMaxFilenameLength";
            this.numMaxFilenameLength.Size = new System.Drawing.Size(70, 20);
            this.numMaxFilenameLength.TabIndex = 7;
            this.numMaxFilenameLength.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(316, 471);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(397, 471);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MCPServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 506);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpToolSettings);
            this.Controls.Add(this.grpAdvancedSettings);
            this.Controls.Add(this.grpLogging);
            this.Controls.Add(this.grpServerSettings);
            this.Controls.Add(this.grpServerControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MCPServerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MCP Server Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MCPServerForm_FormClosing);
            this.grpServerControl.ResumeLayout(false);
            this.grpServerControl.PerformLayout();
            this.grpServerSettings.ResumeLayout(false);
            this.grpServerSettings.PerformLayout();
            this.grpLogging.ResumeLayout(false);
            this.grpLogging.PerformLayout();
            this.grpToolSettings.ResumeLayout(false);
            this.grpToolSettings.PerformLayout();
            this.grpAdvancedSettings.ResumeLayout(false);
            this.grpAdvancedSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRequestSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numShutdownTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFilenameLength)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpServerControl;
        private System.Windows.Forms.Label lblServerStatusLabel;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.GroupBox grpServerSettings;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.CheckBox chkEnableStdio;
        private System.Windows.Forms.CheckBox chkEnableHttp;
        private System.Windows.Forms.Label lblHttpAddress;
        private System.Windows.Forms.TextBox txtHttpAddress;
        private System.Windows.Forms.Label lblHttpPort;
        private System.Windows.Forms.NumericUpDown numHttpPort;
        private System.Windows.Forms.GroupBox grpLogging;
        private System.Windows.Forms.CheckBox chkLoggingEnabled;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.Label lblLogPath;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.GroupBox grpAdvancedSettings;
        private System.Windows.Forms.Label lblMaxRequestSize;
        private System.Windows.Forms.NumericUpDown numMaxRequestSize;
        private System.Windows.Forms.Label lblMaxRequestSizeMB;
        private System.Windows.Forms.Label lblShutdownTimeout;
        private System.Windows.Forms.NumericUpDown numShutdownTimeout;
        private System.Windows.Forms.Label lblShutdownTimeoutMs;
        private System.Windows.Forms.Label lblMaxFilenameLength;
        private System.Windows.Forms.NumericUpDown numMaxFilenameLength;
        private System.Windows.Forms.GroupBox grpToolSettings;
        private System.Windows.Forms.CheckBox chkMemoryReadEnabled;
        private System.Windows.Forms.CheckBox chkMemoryWriteEnabled;
        private System.Windows.Forms.Label lblMemoryWarning;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}
