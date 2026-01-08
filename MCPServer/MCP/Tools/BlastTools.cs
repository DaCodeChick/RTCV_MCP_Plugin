namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;

    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for generating corruption blasts.
    /// </summary>
    public class BlastGenerateHandler : IToolHandler
    {
        public string Name => "blast_generate";
        public string Description => "Generate and execute a corruption blast with current settings";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ToolLogger.Log("Generating blast...");

                    // Parse arguments
                    bool apply = true;
                    if (arguments != null && arguments.ContainsKey("apply"))
                    {
                        apply = Convert.ToBoolean(arguments["apply"]);
                    }

                    BlastLayer blastLayer = null;
                    Exception error = null;

                    // Generate blast on correct thread
                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            blastLayer = RtcCore.GenerateBlastLayerOnAllThreads();
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

                    if (blastLayer == null || blastLayer.Layer.Count == 0)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Failed to generate blast - no units generated (check intensity and selected domains)"
                                }
                            },
                            IsError = false
                        };
                    }

                    int unitCount = blastLayer.Layer.Count;
                    ToolLogger.Log($"Generated blast with {unitCount} units");

                    // Apply if requested
                    if (apply)
                    {
                        SyncObjectSingleton.EmuThreadExecute(() =>
                        {
                            try
                            {
                                blastLayer.Apply(true);
                            }
                            catch (Exception ex)
                            {
                                error = ex;
                            }
                        }, true);

                        if (error != null)
                        {
                            throw error;
                        }

                        ToolLogger.Log($"Applied blast with {unitCount} units");

                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = $"Generated and applied blast with {unitCount} corruption units"
                                }
                            },
                            IsError = false
                        };
                    }
                    else
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = $"Generated blast with {unitCount} corruption units (not applied)"
                                }
                            },
                            IsError = false
                        };
                    }
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error generating blast: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error generating blast: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for toggling AutoCorrupt on/off.
    /// </summary>
    public class BlastToggleHandler : IToolHandler
    {
        public string Name => "blast_toggle";
        public string Description => "Toggle automatic corruption (AutoCorrupt) on or off";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("enabled"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required argument: enabled"
                                }
                            },
                            IsError = true
                        };
                    }

                    bool enabled = Convert.ToBoolean(arguments["enabled"]);
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            RtcCore.AutoCorrupt = enabled;
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

                    string status = enabled ? "enabled" : "disabled";
                    ToolLogger.Log($"AutoCorrupt {status}");

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"AutoCorrupt {status}"
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error toggling AutoCorrupt: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error toggling AutoCorrupt: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for setting blast intensity.
    /// </summary>
    public class BlastSetIntensityHandler : IToolHandler
    {
        public string Name => "blast_set_intensity";
        public string Description => "Set the corruption intensity (blast unit count) and error delay";

        public ToolInputSchema InputSchema => new ToolInputSchema
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

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("intensity"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required argument: intensity"
                                }
                            },
                            IsError = true
                        };
                    }

                    long intensity = Convert.ToInt64(arguments["intensity"]);

                    if (intensity < 1 || intensity > 100000)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Intensity must be between 1 and 100000"
                                }
                            },
                            IsError = true
                        };
                    }

                    long? errorDelay = null;
                    if (arguments.ContainsKey("error_delay"))
                    {
                        errorDelay = Convert.ToInt64(arguments["error_delay"]);
                        if (errorDelay < 10 || errorDelay > 10000)
                        {
                            return new ToolCallResult
                            {
                                Content = new List<ContentBlock>
                                {
                                    new ContentBlock
                                    {
                                        Type = "text",
                                        Text = "Error delay must be between 10 and 10000 milliseconds"
                                    }
                                },
                                IsError = true
                            };
                        }
                    }

                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            RtcCore.Intensity = intensity;
                            if (errorDelay.HasValue)
                            {
                                RtcCore.ErrorDelay = errorDelay.Value;
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

                    ToolLogger.Log($"Set intensity to {intensity}" + (errorDelay.HasValue ? $", error delay to {errorDelay}ms" : ""));

                    string message = $"Set intensity to {intensity}";
                    if (errorDelay.HasValue)
                    {
                        message += $" and error delay to {errorDelay}ms";
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = message
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error setting intensity: {ex.Message}");
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error setting intensity: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
