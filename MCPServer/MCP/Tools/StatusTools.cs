namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using RTCV.Plugins.MCPServer.Logging;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for getting RTCV status information.
    /// </summary>
    public class GetStatusHandler : IToolHandler
    {
        public string Name => "get_status";
        public string Description => "Get current RTCV corruption status and settings";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Logger.Log("Getting RTCV status...", LogLevel.Verbose);

                    StringBuilder status = new StringBuilder();
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            // Core status
                            bool autoCorrupt = RtcCore.AutoCorrupt;
                            long intensity = RtcCore.Intensity;
                            long errorDelay = RtcCore.ErrorDelay;
                            bool attached = RtcCore.Attached;

                            status.AppendLine("## RTCV Status");
                            status.AppendLine();
                            status.AppendLine($"**Connected**: {(attached ? "Yes" : "No")}");
                            status.AppendLine($"**AutoCorrupt**: {(autoCorrupt ? "Enabled" : "Disabled")}");
                            status.AppendLine($"**Intensity**: {intensity}");
                            status.AppendLine($"**Error Delay**: {errorDelay}ms");

                            // Vanguard info (if available)
                            if (AllSpec.VanguardSpec != null)
                            {
                                string gameName = AllSpec.VanguardSpec[VSPEC.GAMENAME] as string ?? "None";
                                string systemName = AllSpec.VanguardSpec[VSPEC.SYSTEM] as string ?? "Unknown";
                                string coreName = AllSpec.VanguardSpec[VSPEC.SYSTEMCORE] as string ?? "Unknown";

                                status.AppendLine();
                                status.AppendLine("## Game Information");
                                status.AppendLine($"**Game**: {gameName}");
                                status.AppendLine($"**System**: {systemName}");
                                status.AppendLine($"**Core**: {coreName}");

                                // Selected domains
                                if (AllSpec.UISpec != null && AllSpec.UISpec[UISPEC.SELECTEDDOMAINS] is string[] selectedDomains)
                                {
                                    status.AppendLine();
                                    status.AppendLine("## Selected Memory Domains");
                                    if (selectedDomains.Length > 0)
                                    {
                                        foreach (var domain in selectedDomains)
                                        {
                                            status.AppendLine($"- {domain}");
                                        }
                                    }
                                    else
                                    {
                                        status.AppendLine("(No domains selected)");
                                    }
                                }
                            }
                            else
                            {
                                status.AppendLine();
                                status.AppendLine("**Note**: No emulator/vanguard connected");
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                    });

                    if (error != null)
                    {
                        throw error;
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = status.ToString()
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error getting status: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error getting status: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for listing available memory domains.
    /// </summary>
    public class MemoryDomainsListHandler : IToolHandler
    {
        public string Name => "memory_domains_list";
        public string Description => "List all available memory domains in the current emulator";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Logger.Log("Listing memory domains...", LogLevel.Verbose);

                    StringBuilder result = new StringBuilder();
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            if (AllSpec.VanguardSpec == null)
                            {
                                result.AppendLine("No emulator/vanguard connected");
                                return;
                            }

                            var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];

                            if (memoryDomainsObj == null)
                            {
                                result.AppendLine("No memory domains available");
                                return;
                            }

                            if (memoryDomainsObj is MemoryDomainProxy[] domains)
                            {
                                result.AppendLine("## Available Memory Domains");
                                result.AppendLine();

                                if (domains.Length == 0)
                                {
                                    result.AppendLine("(No domains found)");
                                }
                                else
                                {
                                    // Get selected domains for comparison
                                    string[] selectedDomains = new string[0];
                                    if (AllSpec.UISpec != null && AllSpec.UISpec[UISPEC.SELECTEDDOMAINS] is string[] selected)
                                    {
                                        selectedDomains = selected;
                                    }

                                    foreach (var domain in domains)
                                    {
                                        string selected_marker = selectedDomains.Contains(domain.Name) ? " [SELECTED]" : "";
                                        result.AppendLine($"**{domain.Name}**{selected_marker}");
                                        result.AppendLine($"  - Size: {domain.Size:N0} bytes ({FormatBytes(domain.Size)})");
                                        result.AppendLine($"  - Word Size: {domain.WordSize} bytes");
                                        result.AppendLine($"  - Endian: {(domain.BigEndian ? "Big" : "Little")}");
                                        result.AppendLine($"  - Writable: {(domain.Writable ? "Yes" : "No")}");
                                        result.AppendLine();
                                    }

                                    result.AppendLine($"**Total Domains**: {domains.Length}");
                                }
                            }
                            else
                            {
                                result.AppendLine($"Unexpected memory domains type: {memoryDomainsObj.GetType().Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                    });

                    if (error != null)
                    {
                        throw error;
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = result.ToString()
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error listing memory domains: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error listing memory domains: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
