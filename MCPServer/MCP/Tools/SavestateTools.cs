namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using RTCV.Plugins.MCPServer.Logging;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for creating savestates.
    /// </summary>
    public class SavestateCreateHandler : IToolHandler
    {
        public string Name => "savestate_create";
        public string Description => "Create a savestate with current emulator state and corruption";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Logger.Log("Creating savestate...", LogLevel.Normal);

                    string name = null;
                    if (arguments != null && arguments.ContainsKey("name"))
                    {
                        name = arguments["name"]?.ToString();
                    }

                    StashKey stashKey = null;
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
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
                            stashKey = StockpileManagerUISide.SaveState();

                            // Set custom alias if provided
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                stashKey.Alias = name;
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

                    if (stashKey == null)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Failed to create savestate"
                                }
                            },
                            IsError = true
                        };
                    }

                    string displayName = stashKey.Alias ?? stashKey.Key;
                    Logger.Log($"Created savestate: {displayName}", LogLevel.Normal);

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Created savestate: {displayName}\nKey: {stashKey.Key}\nGame: {stashKey.GameName}\nSystem: {stashKey.SystemName}"
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error creating savestate: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error creating savestate: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for loading savestates.
    /// Note: This is a simplified implementation. Full stockpile management would require more complex logic.
    /// </summary>
    public class SavestateLoadHandler : IToolHandler
    {
        public string Name => "savestate_load";
        public string Description => "Load a previously saved savestate by key (note: limited functionality in current implementation)";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("key"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required argument: key"
                                }
                            },
                            IsError = true
                        };
                    }

                    string key = arguments["key"]?.ToString();

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Invalid key provided"
                                }
                            },
                            IsError = true
                        };
                    }

                    Logger.Log($"Loading savestate with key: {key}", LogLevel.Normal);

                    // Note: Full implementation would require accessing the stockpile to find the StashKey by key
                    // This is a placeholder that explains the limitation
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = "Savestate loading is not fully implemented in this version. " +
                                       "To load savestates, please use the RTCV UI or add StashKeys to the stockpile first. " +
                                       "Full stockpile integration is planned for a future release."
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error loading savestate: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error loading savestate: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
