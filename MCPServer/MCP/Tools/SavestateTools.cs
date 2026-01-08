namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for creating savestates.
    /// </summary>
    public class SavestateCreateHandler : ToolHandlerBase
    {
        public override string Name => "savestate_create";
        public override string Description => "Create a savestate with current emulator state and corruption";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["name"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Optional name/alias for the savestate"
                }
            }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            string name = GetArgument<string>(arguments, "name", null);

            var stashKey = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                // Check if savestates are supported
                if (AllSpec.VanguardSpec == null)
                {
                    throw new InvalidOperationException("No emulator connected");
                }

                bool supportsSavestates = (bool?)AllSpec.VanguardSpec[VSPEC.SUPPORTS_SAVESTATES] ?? false;

                if (!supportsSavestates)
                {
                    throw new NotSupportedException("Current emulator does not support savestates");
                }

                // Create savestate
                var key = StockpileManagerUISide.SaveState();

                // Set custom alias if provided
                if (!string.IsNullOrWhiteSpace(name))
                {
                    key.Alias = name;
                }

                return key;
            });

            if (stashKey == null)
            {
                return CreateErrorResult("Failed to create savestate");
            }

            string displayName = stashKey.Alias ?? stashKey.Key;
            return CreateSuccessResult(
                $"Created savestate: {displayName}\n" +
                $"Key: {stashKey.Key}\n" +
                $"Game: {stashKey.GameName}\n" +
                $"System: {stashKey.SystemName}"
            );
        }
    }

    /// <summary>
    /// Tool handler for loading savestates.
    /// Note: This is a simplified implementation. Full stockpile management would require more complex logic.
    /// </summary>
    public class SavestateLoadHandler : ToolHandlerBase
    {
        public override string Name => "savestate_load";
        public override string Description => "Load a previously saved savestate by key (note: limited functionality in current implementation)";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["key"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "The savestate key to load"
                }
            },
            Required = new List<string> { "key" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "key");
            
            string key = GetArgument<string>(arguments, "key");

            if (string.IsNullOrWhiteSpace(key))
            {
                return CreateErrorResult("Invalid key provided");
            }

            // Note: Full implementation would require accessing the stockpile to find the StashKey by key
            // This is a placeholder that explains the limitation
            return CreateSuccessResult(
                "Savestate loading is not fully implemented in this version. " +
                "To load savestates, please use the RTCV UI or add StashKeys to the stockpile first. " +
                "Full stockpile integration is planned for a future release."
            );
        }
    }
}
