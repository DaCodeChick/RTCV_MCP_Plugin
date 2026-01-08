using System;
using System.IO;

namespace RTCV.Plugins.MCPServer.Logging
{
    /// <summary>
    /// File-based logger with configurable verbosity levels
    /// </summary>
    public class Logger : IDisposable
    {
        private readonly string logPath;
        private readonly LogLevel level;
        private readonly bool enabled;
        private StreamWriter writer;
        private readonly object lockObject = new object();
        private bool disposed = false;

        /// <summary>
        /// Create a new logger instance
        /// </summary>
        /// <param name="settings">Logging configuration settings</param>
        public Logger(LoggingSettings settings)
        {
            this.enabled = settings.Enabled;
            this.logPath = settings.Path;
            this.level = settings.Level;

            if (enabled)
            {
                try
                {
                    // Ensure log directory exists
                    string directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Open log file for append
                    writer = new StreamWriter(logPath, append: true)
                    {
                        AutoFlush = true
                    };

                    // Write startup marker
                    WriteLog("INFO", "=== MCP Server Logger Started ===");
                }
                catch (Exception ex)
                {
                    // If we can't open log file, disable logging
                    RTCV.Common.Logging.GlobalLogger.Error($"[MCP Server] Failed to open log file: {ex.Message}");
                    enabled = false;
                }
            }
        }

        /// <summary>
        /// Log a minimal level message (errors and critical events only)
        /// </summary>
        public void LogMinimal(string message)
        {
            if (enabled && level >= LogLevel.Minimal)
            {
                WriteLog("MINIMAL", message);
            }
        }

        /// <summary>
        /// Log a normal level message (connection events, tool calls, errors)
        /// </summary>
        public void LogNormal(string message)
        {
            if (enabled && level >= LogLevel.Normal)
            {
                WriteLog("NORMAL", message);
            }
        }

        /// <summary>
        /// Log a verbose level message (all JSON-RPC messages and traces)
        /// </summary>
        public void LogVerbose(string message)
        {
            if (enabled && level >= LogLevel.Verbose)
            {
                WriteLog("VERBOSE", message);
            }
        }

        /// <summary>
        /// Log an error (always logged if enabled)
        /// </summary>
        public void LogError(string message, Exception ex = null)
        {
            if (enabled)
            {
                string fullMessage = message;
                if (ex != null)
                {
                    fullMessage += $"\n{ex}";
                }
                WriteLog("ERROR", fullMessage);
            }
        }

        /// <summary>
        /// Log an informational message at normal level
        /// </summary>
        public void LogInfo(string message)
        {
            LogNormal(message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            if (enabled && level >= LogLevel.Minimal)
            {
                WriteLog("WARNING", message);
            }
        }

        /// <summary>
        /// Write formatted log entry
        /// </summary>
        private void WriteLog(string levelName, string message)
        {
            if (!enabled || writer == null)
                return;

            lock (lockObject)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    writer.WriteLine($"[{timestamp}] [{levelName}] {message}");
                }
                catch (Exception)
                {
                    // Silently fail if write fails (can't log the error!)
                }
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    lock (lockObject)
                    {
                        if (writer != null)
                        {
                            try
                            {
                                WriteLog("INFO", "=== MCP Server Logger Stopped ===");
                                writer.Flush();
                                writer.Dispose();
                                writer = null;
                            }
                            catch (Exception)
                            {
                                // Silently fail on dispose
                            }
                        }
                    }
                }
                disposed = true;
            }
        }

        ~Logger()
        {
            Dispose(false);
        }
    }
}
