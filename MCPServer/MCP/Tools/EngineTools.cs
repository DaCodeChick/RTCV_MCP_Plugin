namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using RTCV.CorruptCore;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for getting corruption engine configuration.
    /// </summary>
    public class EngineGetConfigHandler : ToolHandlerBase
    {
        public override string Name => "engine_get_config";
        public override string Description => "Get current corruption engine configuration";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var config = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("## Corruption Engine Configuration");
                sb.AppendLine();

                // Engine selection
                var engine = RtcCore.SelectedEngine;
                sb.AppendLine($"**Engine**: {engine}");

                // Precision/Alignment
                int precision = RtcCore.CurrentPrecision;
                int alignment = RtcCore.Alignment;
                bool useAlignment = RtcCore.UseAlignment;

                sb.AppendLine($"**Precision**: {precision} byte(s)");
                sb.AppendLine($"**Alignment**: {(useAlignment ? $"{alignment}" : "Disabled")}");

                // Blast settings
                var radius = RtcCore.Radius;
                sb.AppendLine($"**Blast Radius**: {radius}");

                long intensity = RtcCore.Intensity;
                long errorDelay = RtcCore.ErrorDelay;
                sb.AppendLine($"**Intensity**: {intensity}");
                sb.AppendLine($"**Error Delay**: {errorDelay}ms");

                bool autoCorrupt = RtcCore.AutoCorrupt;
                sb.AppendLine($"**AutoCorrupt**: {(autoCorrupt ? "Enabled" : "Disabled")}");

                bool createInfiniteUnits = RtcCore.CreateInfiniteUnits;
                sb.AppendLine($"**Infinite Units**: {(createInfiniteUnits ? "Yes" : "No")}");

                return sb.ToString();
            });

            return CreateSuccessResult(config);
        }
    }

    /// <summary>
    /// Tool handler for setting corruption engine configuration.
    /// </summary>
    public class EngineSetConfigHandler : ToolHandlerBase
    {
        public override string Name => "engine_set_config";
        public override string Description => "Set corruption engine configuration (engine, precision, alignment)";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["engine"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Corruption engine: NIGHTMARE, DISTORTION, FREEZE, PIPE, VECTOR, CLUSTER",
                    ["enum"] = new List<string> { "NIGHTMARE", "DISTORTION", "FREEZE", "PIPE", "VECTOR", "CLUSTER" }
                },
                ["precision"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Byte precision (1, 2, 4, or 8)"
                },
                ["alignment"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Memory alignment (0 to disable, or positive integer)"
                }
            }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            if (arguments.Count == 0)
            {
                return CreateErrorResult("At least one parameter is required (engine, precision, or alignment)");
            }

            var changes = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                var changeList = new List<string>();

                // Set engine if provided
                if (arguments.ContainsKey("engine"))
                {
                    string engineStr = arguments["engine"].ToString().ToUpper();

                    if (Enum.TryParse<CorruptionEngine>(engineStr, out CorruptionEngine engine))
                    {
                        RtcCore.SelectedEngine = engine;
                        changeList.Add($"Engine set to {engine}");
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid engine: {engineStr}. Valid options: NIGHTMARE, DISTORTION, FREEZE, PIPE, VECTOR, CLUSTER");
                    }
                }

                // Set precision if provided
                if (arguments.ContainsKey("precision"))
                {
                    int precision = Convert.ToInt32(arguments["precision"]);

                    if (precision != 1 && precision != 2 && precision != 4 && precision != 8)
                    {
                        throw new ArgumentException("Precision must be 1, 2, 4, or 8 bytes");
                    }

                    RtcCore.CurrentPrecision = precision;
                    changeList.Add($"Precision set to {precision} byte(s)");
                }

                // Set alignment if provided
                if (arguments.ContainsKey("alignment"))
                {
                    int alignment = Convert.ToInt32(arguments["alignment"]);

                    if (alignment < 0)
                    {
                        throw new ArgumentException("Alignment must be 0 (disabled) or a positive integer");
                    }

                    if (alignment == 0)
                    {
                        RtcCore.UseAlignment = false;
                        changeList.Add("Alignment disabled");
                    }
                    else
                    {
                        RtcCore.UseAlignment = true;
                        RtcCore.Alignment = alignment;
                        changeList.Add($"Alignment set to {alignment}");
                    }
                }

                return changeList;
            });

            if (changes.Count == 0)
            {
                return CreateSuccessResult("No changes made");
            }

            return CreateSuccessResult("Configuration updated:\n" + string.Join("\n", changes));
        }
    }
}
