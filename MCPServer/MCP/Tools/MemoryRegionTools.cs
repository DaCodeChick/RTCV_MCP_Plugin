using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTCV.CorruptCore;
using RTCV.NetCore;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    // Helper class for memory region tools
    internal static class MemoryRegionToolHelper
    {
        public static string[] GetTags(Dictionary<string, object> arguments)
        {
            if (!arguments.ContainsKey("tags") || arguments["tags"] == null)
                return null;
            
            if (arguments["tags"] is object[] arr)
                return arr.Select(t => t?.ToString()).ToArray();
            
            return null;
        }
    }

    /// <summary>
    /// Tool for listing/searching memory region annotations
    /// </summary>
    public class ListMemoryRegionsTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public ListMemoryRegionsTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "list_memory_regions";
        public string Description => "List or search memory region annotations by description, tags, or notes for the current emulation target";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Optional search query to filter regions by description, tags, or notes (case-insensitive)"
                }
            },
            Required = new List<string>()
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var target = EmulationTarget.GetCurrent();
                    if (!target.IsValid())
                    {
                        throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
                    }

                    _regionManager.LoadRegions(target);

                    string query = arguments.ContainsKey("query") ? arguments["query"]?.ToString() : null;

                    var regions = string.IsNullOrEmpty(query)
                        ? _regionManager.GetAllRegions()
                        : _regionManager.SearchRegions(query);

                    var sb = new StringBuilder();
                    sb.AppendLine($"## Memory Regions for {target.DisplayName}");
                    sb.AppendLine($"Found {regions.Count} memory region(s):");
                    sb.AppendLine();

                    foreach (var r in regions)
                    {
                        sb.AppendLine($"**ID**: `{r.Id}`");
                        sb.AppendLine($"**Description**: {r.Description}");
                        sb.AppendLine($"**Location**: {r.Domain} @ 0x{r.Address:X} ({r.Size} bytes)");
                        if (!string.IsNullOrEmpty(r.DataType))
                            sb.AppendLine($"**Type**: {r.DataType}");
                        if (r.Tags != null && r.Tags.Length > 0)
                            sb.AppendLine($"**Tags**: {string.Join(", ", r.Tags)}");
                        if (!string.IsNullOrEmpty(r.Notes))
                            sb.AppendLine($"**Notes**: {r.Notes}");
                        sb.AppendLine();
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = sb.ToString() }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error listing memory regions: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool for getting a specific memory region annotation
    /// </summary>
    public class GetMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public GetMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "get_memory_region";
        public string Description => "Get detailed information about a specific memory region annotation by ID";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["id"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Unique identifier of the memory region"
                }
            },
            Required = new List<string> { "id" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string id = arguments.ContainsKey("id") ? arguments["id"]?.ToString() : null;
                    if (string.IsNullOrEmpty(id))
                        throw new ArgumentException("id is required");

                    var r = _regionManager.GetRegion(id);
                    if (r == null)
                        throw new Exception($"Memory region with ID '{id}' not found");

                    var sb = new StringBuilder();
                    sb.AppendLine($"Memory Region: {r.Description}");
                    sb.AppendLine($"ID: {r.Id}");
                    sb.AppendLine($"Domain: {r.Domain}");
                    sb.AppendLine($"Address: 0x{r.Address:X}");
                    sb.AppendLine($"Size: {r.Size} bytes");
                    if (!string.IsNullOrEmpty(r.DataType))
                        sb.AppendLine($"Data Type: {r.DataType}");
                    if (r.Tags != null && r.Tags.Length > 0)
                        sb.AppendLine($"Tags: {string.Join(", ", r.Tags)}");
                    if (!string.IsNullOrEmpty(r.Notes))
                        sb.AppendLine($"Notes: {r.Notes}");
                    sb.AppendLine($"Created: {r.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"Updated: {r.UpdatedAt:yyyy-MM-dd HH:mm:ss}");

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = sb.ToString() }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error getting memory region: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool for updating memory region annotations
    /// </summary>
    public class UpdateMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public UpdateMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "update_memory_region";
        public string Description => "Update an existing memory region annotation";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["id"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Unique identifier of the memory region to update"
                },
                ["description"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Updated description" },
                ["domain"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Updated memory domain" },
                ["address"] = new Dictionary<string, object> { ["type"] = "integer", ["description"] = "Updated start address" },
                ["size"] = new Dictionary<string, object> { ["type"] = "integer", ["description"] = "Updated size in bytes" },
                ["data_type"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Updated data type hint" },
                ["tags"] = new Dictionary<string, object> { ["type"] = "array", ["description"] = "Updated tags" },
                ["notes"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Updated notes" }
            },
            Required = new List<string> { "id" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var target = EmulationTarget.GetCurrent();
                    if (!target.IsValid())
                    {
                        throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
                    }

                    _regionManager.LoadRegions(target);

                    string id = arguments.ContainsKey("id") ? arguments["id"]?.ToString() : null;
                    if (string.IsNullOrEmpty(id))
                        throw new ArgumentException("id is required");

                    string description = arguments.ContainsKey("description") ? arguments["description"]?.ToString() : null;
                    string domain = arguments.ContainsKey("domain") ? arguments["domain"]?.ToString() : null;
                    long? address = arguments.ContainsKey("address") ? (long?)Convert.ToInt64(arguments["address"]) : null;
                    long? size = arguments.ContainsKey("size") ? (long?)Convert.ToInt64(arguments["size"]) : null;
                    string dataType = arguments.ContainsKey("data_type") ? arguments["data_type"]?.ToString() : null;
                    string[] tags = MemoryRegionToolHelper.GetTags(arguments);
                    string notes = arguments.ContainsKey("notes") ? arguments["notes"]?.ToString() : null;

                    bool success = _regionManager.UpdateRegion(target, id, description, domain, address, size, dataType, tags, notes);
                    if (!success)
                        throw new Exception($"Memory region with ID '{id}' not found");

                    var region = _regionManager.GetRegion(id);
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Memory region '{region.Description}' updated successfully" }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error updating memory region: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool for removing memory region annotations
    /// </summary>
    public class RemoveMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public RemoveMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "remove_memory_region";
        public string Description => "Remove a memory region annotation by ID";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["id"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Unique identifier of the memory region to remove"
                }
            },
            Required = new List<string> { "id" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var target = EmulationTarget.GetCurrent();
                    if (!target.IsValid())
                    {
                        throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
                    }

                    _regionManager.LoadRegions(target);

                    string id = arguments.ContainsKey("id") ? arguments["id"]?.ToString() : null;
                    if (string.IsNullOrEmpty(id))
                        throw new ArgumentException("id is required");

                    bool success = _regionManager.RemoveRegion(target, id);
                    if (!success)
                        throw new Exception($"Memory region with ID '{id}' not found");

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Memory region removed successfully" }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error removing memory region: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool for reading memory from an annotated region
    /// </summary>
    public class ReadMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public ReadMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "read_memory_region";
        public string Description => "Read memory data from an annotated region by ID or description query for the current emulation target";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["id"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Region ID (if known)" },
                ["query"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Search query to find region by description" },
                ["offset"] = new Dictionary<string, object> { ["type"] = "integer", ["description"] = "Optional offset from region start address (default: 0)" },
                ["length"] = new Dictionary<string, object> { ["type"] = "integer", ["description"] = "Optional number of bytes to read (default: full region size)" }
            },
            Required = new List<string>()
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var target = EmulationTarget.GetCurrent();
                    if (!target.IsValid())
                    {
                        throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
                    }

                    _regionManager.LoadRegions(target);

                    string id = arguments.ContainsKey("id") ? arguments["id"]?.ToString() : null;
                    string query = arguments.ContainsKey("query") ? arguments["query"]?.ToString() : null;
                    long offset = arguments.ContainsKey("offset") ? Convert.ToInt64(arguments["offset"]) : 0;
                    long? length = arguments.ContainsKey("length") ? (long?)Convert.ToInt64(arguments["length"]) : null;

                    MemoryRegion region = null;
                    if (!string.IsNullOrEmpty(id))
                        region = _regionManager.GetRegion(id);
                    else if (!string.IsNullOrEmpty(query))
                        region = _regionManager.SearchRegions(query).FirstOrDefault();
                    else
                        throw new ArgumentException("Either 'id' or 'query' must be provided");

                    if (region == null)
                        throw new Exception("Memory region not found");

                    long readAddress = region.Address + offset;
                    long readSize = length ?? (region.Size - offset);

                    if (readSize <= 0 || offset >= region.Size)
                        throw new ArgumentException("Invalid offset or length");

                    byte[] data = null;
                    bool success = false;

                    SyncObjectSingleton.EmuThreadExecute(() =>
                    {
                        try
                        {
                            if (AllSpec.VanguardSpec == null)
                                return;

                            var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                            if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                                return;

                            var domain = domains.FirstOrDefault(d => d.Name == region.Domain);
                            if (domain != null)
                            {
                                data = domain.PeekBytes(readAddress, (int)readSize, true);
                                success = true;
                            }
                        }
                        catch { }
                    }, true);

                    if (!success || data == null)
                        throw new Exception($"Failed to read from domain '{region.Domain}'");

                    string hexData = BitConverter.ToString(data).Replace("-", " ");
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Read {data.Length} bytes from '{region.Description}':\n" +
                                      $"Address: 0x{readAddress:X}\n" +
                                      $"Data: {hexData}"
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error reading memory region: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }

    /// <summary>
    /// Tool for writing memory to an annotated region
    /// </summary>
    public class WriteMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public WriteMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "write_memory_region";
        public string Description => "Write memory data to an annotated region by ID or description query for the current emulation target";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["id"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Region ID (if known)" },
                ["query"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "Search query to find region by description" },
                ["data"] = new Dictionary<string, object> { ["type"] = "array", ["description"] = "Byte array to write" },
                ["offset"] = new Dictionary<string, object> { ["type"] = "integer", ["description"] = "Optional offset from region start address (default: 0)" }
            },
            Required = new List<string> { "data" }
        };

        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var target = EmulationTarget.GetCurrent();
                    if (!target.IsValid())
                    {
                        throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
                    }

                    _regionManager.LoadRegions(target);

                    string id = arguments.ContainsKey("id") ? arguments["id"]?.ToString() : null;
                    string query = arguments.ContainsKey("query") ? arguments["query"]?.ToString() : null;
                    byte[] data = arguments.ContainsKey("data") && arguments["data"] is object[] arr 
                        ? arr.Select(b => Convert.ToByte(b)).ToArray() 
                        : null;
                    long offset = arguments.ContainsKey("offset") ? Convert.ToInt64(arguments["offset"]) : 0;

                    if (data == null || data.Length == 0)
                        throw new ArgumentException("data is required and must not be empty");

                    MemoryRegion region = null;
                    if (!string.IsNullOrEmpty(id))
                        region = _regionManager.GetRegion(id);
                    else if (!string.IsNullOrEmpty(query))
                        region = _regionManager.SearchRegions(query).FirstOrDefault();
                    else
                        throw new ArgumentException("Either 'id' or 'query' must be provided");

                    if (region == null)
                        throw new Exception("Memory region not found");

                    long writeAddress = region.Address + offset;
                    if (offset >= region.Size || offset + data.Length > region.Size)
                        throw new ArgumentException($"Data would exceed region bounds");

                    bool success = false;
                    SyncObjectSingleton.EmuThreadExecute(() =>
                    {
                        try
                        {
                            if (AllSpec.VanguardSpec == null)
                                return;

                            var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                            if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                                return;

                            var domain = domains.FirstOrDefault(d => d.Name == region.Domain);
                            if (domain != null)
                            {
                                domain.PokeBytes(writeAddress, data, true);
                                success = true;
                            }
                        }
                        catch { }
                    }, true);

                    if (!success)
                        throw new Exception($"Failed to write to domain '{region.Domain}'");

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Successfully wrote {data.Length} bytes to '{region.Description}' at address 0x{writeAddress:X}"
                            }
                        },
                        IsError = false
                    };
                }
                catch (Exception ex)
                {
                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent { Type = "text", Text = $"Error writing memory region: {ex.Message}" }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
