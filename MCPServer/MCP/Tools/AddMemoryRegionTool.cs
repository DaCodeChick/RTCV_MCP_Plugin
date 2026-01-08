using System;
using System.Collections.Generic;
using System.Linq;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// MCP tool for adding memory region annotations
    /// </summary>
    public class AddMemoryRegionTool : MemoryRegionToolBase
    {
        public AddMemoryRegionTool(MemoryRegionManager regionManager) 
            : base(regionManager)
        {
        }

        public override string Name => "add_memory_region";

        public override string Description => "Annotate a memory region with a semantic description for AI-assisted memory manipulation on the current emulation target";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();

            ValidateRequiredArgument(arguments, "description");
            ValidateRequiredArgument(arguments, "domain");
            ValidateRequiredArgument(arguments, "address");
            ValidateRequiredArgument(arguments, "size");

            string description = GetArgument<string>(arguments, "description");
            string domain = GetArgument<string>(arguments, "domain");
            long address = Convert.ToInt64(arguments["address"]);
            long size = Convert.ToInt64(arguments["size"]);
            string dataType = GetArgument<string>(arguments, "data_type", null);
            string[] tags = GetTags(arguments);
            string notes = GetArgument<string>(arguments, "notes", null);

            if (size <= 0)
            {
                return CreateErrorResult("size must be greater than 0");
            }

            // Add the region
            var region = RegionManager.AddRegion(target, description, domain, address, size, 
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

            return CreateSuccessResult(resultText);
        }
    }
}
