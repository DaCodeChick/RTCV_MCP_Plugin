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
    /// Tool handler for adding current state to stockpile history.
    /// </summary>
    public class StockpileAddHandler : IToolHandler
    {
        public string Name => "stockpile_add";
        public string Description => "Add current corruption state to stockpile history/stash";

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
                    Logger.Log("Adding current state to stockpile...", LogLevel.Normal);

                    bool added = false;
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            added = StockpileManagerUISide.AddCurrentStashkeyToStash(true);
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

                    if (!added)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "No corruption applied - nothing to add to stockpile"
                                }
                            },
                            IsError = false
                        };
                    }

                    Logger.Log("Added current state to stockpile", LogLevel.Normal);

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = "Successfully added current corruption state to stockpile"
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error adding to stockpile: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error adding to stockpile: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for applying stored corruption from stockpile.
    /// Note: This is a simplified implementation.
    /// </summary>
    public class StockpileApplyHandler : IToolHandler
    {
        public string Name => "stockpile_apply";
        public string Description => "Apply corruption from stockpile (note: limited functionality in current implementation)";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("index"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required argument: index"
                                }
                            },
                            IsError = true
                        };
                    }

                    int index = Convert.ToInt32(arguments["index"]);

                    Logger.Log($"Attempting to apply stockpile item at index {index}", LogLevel.Normal);

                    // Note: Full implementation would require iterating through StashHistory and applying the specific item
                    // This is a placeholder
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = "Stockpile apply is not fully implemented in this version. " +
                                       "To apply stockpile items, please use the RTCV UI. " +
                                       "Full stockpile integration is planned for a future release."
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error applying from stockpile: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error applying from stockpile: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
