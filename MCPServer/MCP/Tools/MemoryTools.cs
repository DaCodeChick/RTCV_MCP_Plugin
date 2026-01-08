namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using RTCV.Plugins.MCPServer.Logging;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for reading memory values.
    /// WARNING: This tool is disabled by default for safety.
    /// </summary>
    public class MemoryReadHandler : IToolHandler
    {
        public string Name => "memory_read";
        public string Description => "Read memory values from a specific domain and address";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["domain"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Memory domain name (e.g., 'System Bus', 'Main RAM')"
                },
                ["address"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Memory address to read from"
                },
                ["length"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Number of bytes to read (1-1024)"
                }
            },
            Required = new List<string> { "domain", "address", "length" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("domain") || 
                        !arguments.ContainsKey("address") || !arguments.ContainsKey("length"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required arguments: domain, address, length"
                                }
                            },
                            IsError = true
                        };
                    }

                    string domainName = arguments["domain"]?.ToString();
                    long address = Convert.ToInt64(arguments["address"]);
                    int length = Convert.ToInt32(arguments["length"]);

                    if (length < 1 || length > 1024)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Length must be between 1 and 1024 bytes"
                                }
                            },
                            IsError = true
                        };
                    }

                    Logger.Log($"Reading {length} bytes from {domainName}:0x{address:X}", LogLevel.Verbose);

                    byte[] data = null;
                    Exception error = null;

                    SyncObjectSingleton.FormExecute(() =>
                    {
                        try
                        {
                            if (AllSpec.VanguardSpec == null)
                            {
                                throw new InvalidOperationException("No emulator connected");
                            }

                            var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                            if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                            {
                                throw new InvalidOperationException("No memory domains available");
                            }

                            var domain = domains.FirstOrDefault(d => d.Name == domainName);
                            if (domain == null)
                            {
                                throw new ArgumentException($"Memory domain '{domainName}' not found");
                            }

                            if (address < 0 || address + length > domain.Size)
                            {
                                throw new ArgumentOutOfRangeException($"Address range 0x{address:X}-0x{address + length:X} is outside domain bounds (0x0-0x{domain.Size:X})");
                            }

                            data = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                data[i] = domain.PeekByte(address + i);
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

                    // Format output as hex dump
                    string hexString = BitConverter.ToString(data).Replace("-", " ");
                    string result = $"Read {length} bytes from {domainName}:0x{address:X}\n\nHex: {hexString}";

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = result
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error reading memory: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error reading memory: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool handler for writing memory values.
    /// WARNING: This tool is disabled by default for safety.
    /// </summary>
    public class MemoryWriteHandler : IToolHandler
    {
        public string Name => "memory_write";
        public string Description => "Write memory values to a specific domain and address";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["domain"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Memory domain name (e.g., 'System Bus', 'Main RAM')"
                },
                ["address"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Memory address to write to"
                },
                ["data"] = new Dictionary<string, object>
                {
                    ["type"] = "array",
                    ["description"] = "Array of byte values to write (0-255)",
                    ["items"] = new Dictionary<string, object>
                    {
                        ["type"] = "number"
                    }
                }
            },
            Required = new List<string> { "domain", "address", "data" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (arguments == null || !arguments.ContainsKey("domain") || 
                        !arguments.ContainsKey("address") || !arguments.ContainsKey("data"))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Missing required arguments: domain, address, data"
                                }
                            },
                            IsError = true
                        };
                    }

                    string domainName = arguments["domain"]?.ToString();
                    long address = Convert.ToInt64(arguments["address"]);
                    
                    // Parse data array
                    if (!(arguments["data"] is IEnumerable<object> dataObjects))
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Data must be an array of numbers"
                                }
                            },
                            IsError = true
                        };
                    }

                    byte[] data = dataObjects.Select(o => Convert.ToByte(o)).ToArray();

                    if (data.Length < 1 || data.Length > 1024)
                    {
                        return new ToolCallResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new ContentBlock
                                {
                                    Type = "text",
                                    Text = "Data length must be between 1 and 1024 bytes"
                                }
                            },
                            IsError = true
                        };
                    }

                    Logger.Log($"Writing {data.Length} bytes to {domainName}:0x{address:X}", LogLevel.Normal);

                    Exception error = null;

                    SyncObjectSingleton.EmuThreadExecute(() =>
                    {
                        try
                        {
                            if (AllSpec.VanguardSpec == null)
                            {
                                throw new InvalidOperationException("No emulator connected");
                            }

                            var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                            if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                            {
                                throw new InvalidOperationException("No memory domains available");
                            }

                            var domain = domains.FirstOrDefault(d => d.Name == domainName);
                            if (domain == null)
                            {
                                throw new ArgumentException($"Memory domain '{domainName}' not found");
                            }

                            if (!domain.Writable)
                            {
                                throw new InvalidOperationException($"Memory domain '{domainName}' is not writable");
                            }

                            if (address < 0 || address + data.Length > domain.Size)
                            {
                                throw new ArgumentOutOfRangeException($"Address range 0x{address:X}-0x{address + data.Length:X} is outside domain bounds (0x0-0x{domain.Size:X})");
                            }

                            for (int i = 0; i < data.Length; i++)
                            {
                                domain.PokeByte(address + i, data[i]);
                            }
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

                    string hexString = BitConverter.ToString(data).Replace("-", " ");
                    string result = $"Wrote {data.Length} bytes to {domainName}:0x{address:X}\n\nHex: {hexString}";

                    Logger.Log($"Successfully wrote {data.Length} bytes", LogLevel.Normal);

                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = result
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error writing memory: {ex.Message}", LogLevel.Minimal);
                    return new ToolCallResult
                    {
                        Content = new List<ContentBlock>
                        {
                            new ContentBlock
                            {
                                Type = "text",
                                Text = $"Error writing memory: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
