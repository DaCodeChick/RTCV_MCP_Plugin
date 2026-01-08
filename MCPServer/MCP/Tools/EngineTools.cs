namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for getting corruption engine configuration.
    /// </summary>
    public class EngineGetConfigHandler : IToolHandler
    {
        public string Name => "engine_get_config";
        public string Description => "Get current corruption engine configuration";

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
                    ToolLogger.Log("Getting engine configuration...");

                    StringBuilder config = new StringBuilder();
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            config.AppendLine("## Corruption Engine Configuration");
                            config.AppendLine();

                            // Engine selection
                            var engine = RtcCore.SelectedEngine;
                            config.AppendLine($"**Engine**: {engine}");

                            // Precision/Alignment
                            int precision = RtcCore.CurrentPrecision;
                            int alignment = RtcCore.Alignment;
                            bool useAlignment = RtcCore.UseAlignment;

                            config.AppendLine($"**Precision**: {precision} byte(s)");
                            config.AppendLine($"**Alignment**: {(useAlignment ? $"{alignment}" : "Disabled")}");

                            // Blast settings
                            var radius = RtcCore.Radius;
                            config.AppendLine($"**Blast Radius**: {radius}");

                            long intensity = RtcCore.Intensity;
                            long errorDelay = RtcCore.ErrorDelay;
                            config.AppendLine($"**Intensity**: {intensity}");
                            config.AppendLine($"**Error Delay**: {errorDelay}ms");

                            bool autoCorrupt = RtcCore.AutoCorrupt;
                            config.AppendLine($"**AutoCorrupt**: {(autoCorrupt ? "Enabled" : "Disabled")}");

                            bool createInfiniteUnits = RtcCore.CreateInfiniteUnits;
                            config.AppendLine($"**Infinite Units**: {(createInfiniteUnits ? "Yes" : "No")}");
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
                                Text = config.ToString()
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error getting engine config: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Error getting engine config: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for setting corruption engine configuration.
    /// </summary>
    public class EngineSetConfigHandler : IToolHandler
    {
        public string Name => "engine_set_config";
        public string Description => "Set corruption engine configuration (engine, precision, alignment)";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || arguments.Count == 0)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ToolContent>
                            {
                                new ToolContent
                                {
                                    Type = "text",
                                    Text = "At least one parameter is required (engine, precision, or alignment)"
                                }
                            },
                            IsError = true
                        };
                    }

                    List<string> changes = new List<string>();
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            // Set engine if provided
                            if (arguments.ContainsKey("engine"))
                            {
                                string engineStr = arguments["engine"].ToString().ToUpper();

                                if (Enum.TryParse<CorruptionEngine>(engineStr, out CorruptionEngine engine))
                                {
                                    RtcCore.SelectedEngine = engine;
                                    changes.Add($"Engine set to {engine}");
                                    ToolLogger.Log($"Engine set to {engine}");
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
                                changes.Add($"Precision set to {precision} byte(s)");
                                ToolLogger.Log($"Precision set to {precision}");
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
                                    changes.Add("Alignment disabled");
                                    ToolLogger.Log("Alignment disabled");
                                }
                                else
                                {
                                    RtcCore.UseAlignment = true;
                                    RtcCore.Alignment = alignment;
                                    changes.Add($"Alignment set to {alignment}");
                                    ToolLogger.Log($"Alignment set to {alignment}");
                                }
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

                    if (changes.Count == 0)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ToolContent>
                            {
                                new ToolContent
                                {
                                    Type = "text",
                                    Text = "No changes made"
                                }
                            },
                            IsError = false
                        };
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = "Configuration updated:\n" + string.Join("\n", changes)
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error setting engine config: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Error setting engine config: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
