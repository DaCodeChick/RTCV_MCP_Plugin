namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using Newtonsoft.Json;
    
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
                    ToolLogger.Log("Getting RTCV status...");

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
                        Content = new List<ToolContent>
                        {
                            new ToolContent
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
                    ToolLogger.LogError($"Error getting status: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
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
                    ToolLogger.Log("Listing memory domains...");

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
                                        result.AppendLine($"  - Writable: {(!domain.ReadOnly ? "Yes" : "No")}");
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
                        Content = new List<ToolContent>
                        {
                            new ToolContent
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
                    ToolLogger.LogError($"Error listing memory domains: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
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

    /// <summary>
    /// Tool handler for getting emulation target information.
    /// </summary>
    public class GetEmulationTargetHandler : IToolHandler
    {
        public string Name => "get_emulation_target_info";
        public string Description => "Get information about the current emulation target (system, game, core)";

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
                    ToolLogger.Log("Getting emulation target info...");

                    var target = EmulationTarget.GetCurrent();

                    // Format as JSON
                    string json = JsonConvert.SerializeObject(target, Formatting.Indented);

                    // Also format as human-readable markdown
                    StringBuilder markdown = new StringBuilder();
                    markdown.AppendLine("## Emulation Target Information");
                    markdown.AppendLine();
                    markdown.AppendLine($"**Status**: {(target.Attached ? "Connected" : "Not Connected")}");
                    
                    if (target.Attached)
                    {
                        markdown.AppendLine($"**Game**: {target.GameName}");
                        markdown.AppendLine($"**System**: {target.System}");
                        markdown.AppendLine($"**Core**: {target.Core}");
                        markdown.AppendLine($"**Vanguard**: {target.VanguardName}");
                        markdown.AppendLine();
                        markdown.AppendLine($"**Valid Target**: {(target.IsValid() ? "Yes" : "No (no game loaded)")}");
                        markdown.AppendLine($"**Storage ID**: `{target.FilenameSafeId}`");
                    }
                    else
                    {
                        markdown.AppendLine();
                        markdown.AppendLine("*No emulator currently attached. Connect an emulator to RTCV to see target information.*");
                    }

                    markdown.AppendLine();
                    markdown.AppendLine("### JSON Data");
                    markdown.AppendLine("```json");
                    markdown.AppendLine(json);
                    markdown.AppendLine("```");

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = markdown.ToString()
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error getting emulation target info: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Error getting emulation target info: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
