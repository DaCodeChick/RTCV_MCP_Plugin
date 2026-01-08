namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using RTCV.CorruptCore;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for adding current state to stockpile history.
    /// </summary>
    public class StockpileAddHandler : ToolHandlerBase
    {
        public override string Name => "stockpile_add";
        public override string Description => "Add current corruption state to stockpile history/stash";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>()
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            bool added = RtcvThreadHelper.ExecuteOnFormThread(() => 
                StockpileManagerUISide.AddCurrentStashkeyToStash(true)
            );

            if (!added)
            {
                return CreateSuccessResult("No corruption applied - nothing to add to stockpile");
            }

            return CreateSuccessResult("Successfully added current corruption state to stockpile");
        }
    }

    /// <summary>
    /// Tool handler for applying stored corruption from stockpile.
    /// Note: This is a simplified implementation.
    /// </summary>
    public class StockpileApplyHandler : ToolHandlerBase
    {
        public override string Name => "stockpile_apply";
        public override string Description => "Apply corruption from stockpile (note: limited functionality in current implementation)";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["index"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Index of stockpile item to apply (0-based)"
                }
            },
            Required = new List<string> { "index" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "index");
            
            int index = GetArgument<int>(arguments, "index");

            // Note: Full implementation would require iterating through StashHistory and applying the specific item
            // This is a placeholder
            return CreateSuccessResult(
                "Stockpile apply is not fully implemented in this version. " +
                "To apply stockpile items, please use the RTCV UI. " +
                "Full stockpile integration is planned for a future release."
            );
        }
    }
}
