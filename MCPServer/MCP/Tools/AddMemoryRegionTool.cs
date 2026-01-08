using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// MCP tool for adding memory region annotations
    /// </summary>
    public class AddMemoryRegionTool : IToolHandler
    {
        private readonly MemoryRegionManager _regionManager;

        public AddMemoryRegionTool(MemoryRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public string Name => "add_memory_region";

        public string Description => "Annotate a memory region with a semantic description for AI-assisted memory manipulation on the current emulation target";

        public ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["description"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Human-readable description (e.g., 'Mario sprite palette', 'Player health')"
                },
                ["domain"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Memory domain (e.g., 'WRAM', 'SRAM', 'ROM')"
                },
                ["address"] = new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = "Start address of the memory region"
                },
                ["size"] = new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = "Size of the region in bytes"
                },
                ["data_type"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Optional data type hint (e.g., 'palette', 'sprite', 'text', 'integer', 'float')"
                },
                ["tags"] = new Dictionary<string, object>
                {
                    ["type"] = "array",
                    ["items"] = new Dictionary<string, object> { ["type"] = "string" },
                    ["description"] = "Optional tags for categorization (e.g., ['sprite', 'mario', 'palette'])"
                },
                ["notes"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Optional additional notes about the region"
                }
            },
            Required = new List<string> { "description", "domain", "address", "size" }
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

                    string description = arguments.ContainsKey("description") ? arguments["description"]?.ToString() : null;
                    string domain = arguments.ContainsKey("domain") ? arguments["domain"]?.ToString() : null;
                    long address = arguments.ContainsKey("address") ? Convert.ToInt64(arguments["address"]) : 0;
                    long size = arguments.ContainsKey("size") ? Convert.ToInt64(arguments["size"]) : 0;
                    string dataType = arguments.ContainsKey("data_type") ? arguments["data_type"]?.ToString() : null;
                    string[] tags = arguments.ContainsKey("tags") && arguments["tags"] is object[] arr ? arr.Select(t => t?.ToString()).ToArray() : null;
                    string notes = arguments.ContainsKey("notes") ? arguments["notes"]?.ToString() : null;

                    if (string.IsNullOrEmpty(description))
                        throw new ArgumentException("description is required");
                    if (string.IsNullOrEmpty(domain))
                        throw new ArgumentException("domain is required");
                    if (size <= 0)
                        throw new ArgumentException("size must be greater than 0");

                    // Add the region
                    var region = _regionManager.AddRegion(target, description, domain, address, size, 
                        dataType, tags, notes);

                    string resultText = $"Memory region added successfully for {target.DisplayName}\n" +
                                       $"ID: {region.Id}\n" +
                                       $"Description: {description}\n" +
                                       $"Domain: {region.Domain}\n" +
                                       $"Address: 0x{region.Address:X}\n" +
                                       $"Size: {region.Size} bytes";

                    if (tags != null && tags.Length > 0)
                    {
                        resultText += $"\nTags: {string.Join(", ", tags)}";
                    }

                    return new ToolCallResult
                    {
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = resultText
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
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Error adding memory region: {ex.Message}"
                            }
                        },
                        IsError = true
                    };
                }
            });
        }
    }
}
