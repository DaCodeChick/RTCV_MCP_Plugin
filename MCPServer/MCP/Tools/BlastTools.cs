namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using RTCV.CorruptCore;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for generating corruption blasts.
    /// </summary>
    public class BlastGenerateHandler : ToolHandlerBase
    {
        public override string Name => "blast_generate";
        public override string Description => "Generate and execute a corruption blast with current settings";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["apply"] = new Dictionary<string, object>
                {
                    ["type"] = "boolean",
                    ["description"] = "Whether to apply the blast immediately (default: true)"
                }
            }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            bool apply = GetArgument(arguments, "apply", true);

            var blastLayer = RtcvThreadHelper.ExecuteOnFormThread(() => 
                RtcCore.GenerateBlastLayerOnAllThreads()
            );

            if (blastLayer == null || blastLayer.Layer.Count == 0)
            {
                return CreateSuccessResult(
                    "Failed to generate blast - no units generated (check intensity and selected domains)"
                );
            }

            int unitCount = blastLayer.Layer.Count;

            if (apply)
            {
                RtcvThreadHelper.ExecuteOnEmuThread(() => 
                    blastLayer.Apply(true)
                );

                return CreateSuccessResult(
                    $"Generated and applied blast with {unitCount} corruption units"
                );
            }

            return CreateSuccessResult(
                $"Generated blast with {unitCount} corruption units (not applied)"
            );
        }
    }

    /// <summary>
    /// Tool handler for toggling AutoCorrupt on/off.
    /// </summary>
    public class BlastToggleHandler : ToolHandlerBase
    {
        public override string Name => "blast_toggle";
        public override string Description => "Toggle automatic corruption (AutoCorrupt) on or off";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["enabled"] = new Dictionary<string, object>
                {
                    ["type"] = "boolean",
                    ["description"] = "Enable (true) or disable (false) AutoCorrupt"
                }
            },
            Required = new List<string> { "enabled" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "enabled");
            
            bool enabled = GetArgument<bool>(arguments, "enabled");

            RtcvThreadHelper.ExecuteOnFormThread(() => 
                RtcCore.AutoCorrupt = enabled
            );

            string status = enabled ? "enabled" : "disabled";
            return CreateSuccessResult($"AutoCorrupt {status}");
        }
    }

    /// <summary>
    /// Tool handler for setting blast intensity.
    /// </summary>
    public class BlastSetIntensityHandler : ToolHandlerBase
    {
        public override string Name => "blast_set_intensity";
        public override string Description => "Set the corruption intensity (blast unit count) and error delay";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["intensity"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Number of corruption units per blast (1-100000)"
                },
                ["error_delay"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Milliseconds between auto-corruption blasts (10-10000, optional)"
                }
            },
            Required = new List<string> { "intensity" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "intensity");
            
            long intensity = Convert.ToInt64(arguments["intensity"]);

            if (intensity < 1 || intensity > 100000)
            {
                return CreateErrorResult("Intensity must be between 1 and 100000");
            }

            long? errorDelay = null;
            if (arguments.ContainsKey("error_delay"))
            {
                errorDelay = Convert.ToInt64(arguments["error_delay"]);
                if (errorDelay < 10 || errorDelay > 10000)
                {
                    return CreateErrorResult("Error delay must be between 10 and 10000 milliseconds");
                }
            }

            RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                RtcCore.Intensity = intensity;
                if (errorDelay.HasValue)
                {
                    RtcCore.ErrorDelay = errorDelay.Value;
                }
            });

            string message = $"Set intensity to {intensity}";
            if (errorDelay.HasValue)
            {
                message += $" and error delay to {errorDelay}ms";
            }

            return CreateSuccessResult(message);
        }
    }
}
