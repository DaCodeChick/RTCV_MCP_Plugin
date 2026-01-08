namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using Newtonsoft.Json;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for getting RTCV status information.
    /// </summary>
    public class GetStatusHandler : ToolHandlerBase
    {
        public override string Name => "get_status";
        public override string Description => "Get current RTCV corruption status and settings";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var status = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                var sb = new StringBuilder();

                // Core status
                bool autoCorrupt = RtcCore.AutoCorrupt;
                long intensity = RtcCore.Intensity;
                long errorDelay = RtcCore.ErrorDelay;
                bool attached = RtcCore.Attached;

                sb.AppendLine("## RTCV Status");
                sb.AppendLine();
                sb.AppendLine($"**Connected**: {(attached ? "Yes" : "No")}");
                sb.AppendLine($"**AutoCorrupt**: {(autoCorrupt ? "Enabled" : "Disabled")}");
                sb.AppendLine($"**Intensity**: {intensity}");
                sb.AppendLine($"**Error Delay**: {errorDelay}ms");

                // Vanguard info (if available)
                if (AllSpec.VanguardSpec != null)
                {
                    string gameName = AllSpec.VanguardSpec[VSPEC.GAMENAME] as string ?? "None";
                    string systemName = AllSpec.VanguardSpec[VSPEC.SYSTEM] as string ?? "Unknown";
                    string coreName = AllSpec.VanguardSpec[VSPEC.SYSTEMCORE] as string ?? "Unknown";

                    sb.AppendLine();
                    sb.AppendLine("## Game Information");
                    sb.AppendLine($"**Game**: {gameName}");
                    sb.AppendLine($"**System**: {systemName}");
                    sb.AppendLine($"**Core**: {coreName}");

                    // Selected domains
                    if (AllSpec.UISpec != null && AllSpec.UISpec[UISPEC.SELECTEDDOMAINS] is string[] selectedDomains)
                    {
                        sb.AppendLine();
                        sb.AppendLine("## Selected Memory Domains");
                        if (selectedDomains.Length > 0)
                        {
                            foreach (var domain in selectedDomains)
                            {
                                sb.AppendLine($"- {domain}");
                            }
                        }
                        else
                        {
                            sb.AppendLine("(No domains selected)");
                        }
                    }
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("**Note**: No emulator/vanguard connected");
                }

                return sb.ToString();
            });

            return CreateSuccessResult(status);
        }
    }

    /// <summary>
    /// Tool handler for listing available memory domains.
    /// </summary>
    public class MemoryDomainsListHandler : ToolHandlerBase
    {
        public override string Name => "memory_domains_list";
        public override string Description => "List all available memory domains in the current emulator";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var result = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                var sb = new StringBuilder();

                if (AllSpec.VanguardSpec == null)
                {
                    return "No emulator/vanguard connected";
                }

                var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];

                if (memoryDomainsObj == null)
                {
                    return "No memory domains available";
                }

                if (memoryDomainsObj is MemoryDomainProxy[] domains)
                {
                    sb.AppendLine("## Available Memory Domains");
                    sb.AppendLine();

                    if (domains.Length == 0)
                    {
                        sb.AppendLine("(No domains found)");
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
                            sb.AppendLine($"**{domain.Name}**{selected_marker}");
                            sb.AppendLine($"  - Size: {domain.Size:N0} bytes ({FormatBytes(domain.Size)})");
                            sb.AppendLine($"  - Word Size: {domain.WordSize} bytes");
                            sb.AppendLine($"  - Endian: {(domain.BigEndian ? "Big" : "Little")}");
                            sb.AppendLine($"  - Writable: {(!domain.ReadOnly ? "Yes" : "No")}");
                            sb.AppendLine();
                        }

                        sb.AppendLine($"**Total Domains**: {domains.Length}");
                    }
                }
                else
                {
                    return $"Unexpected memory domains type: {memoryDomainsObj.GetType().Name}";
                }

                return sb.ToString();
            });

            return CreateSuccessResult(result);
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
    public class GetEmulationTargetHandler : ToolHandlerBase
    {
        public override string Name => "get_emulation_target_info";
        public override string Description => "Get information about the current emulation target (system, game, core)";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EmulationTarget.GetCurrent();

            // Format as JSON
            string json = JsonConvert.SerializeObject(target, Formatting.Indented);

            // Also format as human-readable markdown
            var markdown = new StringBuilder();
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

            return CreateSuccessResult(markdown.ToString());
        }
    }
}
