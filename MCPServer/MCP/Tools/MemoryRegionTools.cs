using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTCV.CorruptCore;
using RTCV.NetCore;
using RTCV.Plugins.MCPServer.Helpers;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// Tool for listing/searching memory region annotations
    /// </summary>
    public class ListMemoryRegionsTool : MemoryRegionToolBase
    {
        public ListMemoryRegionsTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "list_memory_regions";
        public override string Description => "List or search memory region annotations by description, tags, or notes for the current emulation target";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();

            string query = GetArgument<string>(arguments, "query", null);

            var regions = string.IsNullOrEmpty(query)
                ? RegionManager.GetAllRegions()
                : RegionManager.SearchRegions(query);

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

            return CreateSuccessResult(sb.ToString());
        }
    }

    /// <summary>
    /// Tool for getting a specific memory region annotation
    /// </summary>
    public class GetMemoryRegionTool : MemoryRegionToolBase
    {
        public GetMemoryRegionTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "get_memory_region";
        public override string Description => "Get detailed information about a specific memory region annotation by ID";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "id");
            string id = GetArgument<string>(arguments, "id", null);

            var r = RegionManager.GetRegion(id);
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

            return CreateSuccessResult(sb.ToString());
        }
    }

    /// <summary>
    /// Tool for updating memory region annotations
    /// </summary>
    public class UpdateMemoryRegionTool : MemoryRegionToolBase
    {
        public UpdateMemoryRegionTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "update_memory_region";
        public override string Description => "Update an existing memory region annotation";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();

            ValidateRequiredArgument(arguments, "id");
            string id = GetArgument<string>(arguments, "id", null);
            string description = GetArgument<string>(arguments, "description", null);
            string domain = GetArgument<string>(arguments, "domain", null);
            long? address = GetArgument<long?>(arguments, "address", null);
            long? size = GetArgument<long?>(arguments, "size", null);
            string dataType = GetArgument<string>(arguments, "data_type", null);
            string[] tags = GetTags(arguments);
            string notes = GetArgument<string>(arguments, "notes", null);

            bool success = RegionManager.UpdateRegion(target, id, description, domain, address, size, dataType, tags, notes);
            if (!success)
                throw new Exception($"Memory region with ID '{id}' not found");

            var region = RegionManager.GetRegion(id);
            return CreateSuccessResult($"Memory region '{region.Description}' updated successfully");
        }
    }

    /// <summary>
    /// Tool for removing memory region annotations
    /// </summary>
    public class RemoveMemoryRegionTool : MemoryRegionToolBase
    {
        public RemoveMemoryRegionTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "remove_memory_region";
        public override string Description => "Remove a memory region annotation by ID";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();
            string id = GetArgument<string>(arguments, "id", null);

            bool success = RegionManager.RemoveRegion(target, id);
            if (!success)
                throw new Exception($"Memory region with ID '{id}' not found");

            return CreateSuccessResult("Memory region removed successfully");
        }
    }

    /// <summary>
    /// Tool for reading memory from an annotated region
    /// </summary>
    public class ReadMemoryRegionTool : MemoryRegionToolBase
    {
        public ReadMemoryRegionTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "read_memory_region";
        public override string Description => "Read memory data from an annotated region by ID or description query for the current emulation target";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();

            string id = GetArgument<string>(arguments, "id", null);
            string query = GetArgument<string>(arguments, "query", null);
            long offset = GetArgument<long>(arguments, "offset", 0);
            long? length = GetArgument<long?>(arguments, "length", null);

            MemoryRegion region = null;
            if (!string.IsNullOrEmpty(id))
                region = RegionManager.GetRegion(id);
            else if (!string.IsNullOrEmpty(query))
                region = RegionManager.SearchRegions(query).FirstOrDefault();
            else
                throw new ArgumentException("Either 'id' or 'query' must be provided");

            if (region == null)
                throw new Exception("Memory region not found");

            long readAddress = region.Address + offset;
            long readSize = length ?? (region.Size - offset);

            if (readSize <= 0 || offset >= region.Size)
                throw new ArgumentException("Invalid offset or length");

            byte[] data = RtcvThreadHelper.ExecuteOnEmuThread(() =>
            {
                if (AllSpec.VanguardSpec == null)
                    throw new InvalidOperationException("VanguardSpec not available");

                var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                    throw new InvalidOperationException("Memory domains not available");

                var domain = domains.FirstOrDefault(d => d.Name == region.Domain);
                if (domain == null)
                    throw new Exception($"Memory domain '{region.Domain}' not found");

                return domain.PeekBytes(readAddress, (int)readSize, true);
            });

            if (data == null)
                throw new Exception($"Failed to read from domain '{region.Domain}'");

            string hexData = BitConverter.ToString(data).Replace("-", " ");
            return CreateSuccessResult($"Read {data.Length} bytes from '{region.Description}':\n" +
                                      $"Address: 0x{readAddress:X}\n" +
                                      $"Data: {hexData}");
        }
    }

    /// <summary>
    /// Tool for writing memory to an annotated region
    /// </summary>
    public class WriteMemoryRegionTool : MemoryRegionToolBase
    {
        public WriteMemoryRegionTool(MemoryRegionManager regionManager)
            : base(regionManager)
        {
        }

        public override string Name => "write_memory_region";
        public override string Description => "Write memory data to an annotated region by ID or description query for the current emulation target";

        public override ToolInputSchema InputSchema => new ToolInputSchema
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

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            var target = EnsureValidTarget();

            string id = GetArgument<string>(arguments, "id", null);
            string query = GetArgument<string>(arguments, "query", null);
            byte[] data = arguments.ContainsKey("data") && arguments["data"] is object[] arr 
                ? arr.Select(b => Convert.ToByte(b)).ToArray() 
                : null;
            long offset = GetArgument<long>(arguments, "offset", 0);

            if (data == null || data.Length == 0)
                throw new ArgumentException("data is required and must not be empty");

            MemoryRegion region = null;
            if (!string.IsNullOrEmpty(id))
                region = RegionManager.GetRegion(id);
            else if (!string.IsNullOrEmpty(query))
                region = RegionManager.SearchRegions(query).FirstOrDefault();
            else
                throw new ArgumentException("Either 'id' or 'query' must be provided");

            if (region == null)
                throw new Exception("Memory region not found");

            long writeAddress = region.Address + offset;
            if (offset >= region.Size || offset + data.Length > region.Size)
                throw new ArgumentException($"Data would exceed region bounds");

            RtcvThreadHelper.ExecuteOnEmuThread(() =>
            {
                if (AllSpec.VanguardSpec == null)
                    throw new InvalidOperationException("VanguardSpec not available");

                var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                    throw new InvalidOperationException("Memory domains not available");

                var domain = domains.FirstOrDefault(d => d.Name == region.Domain);
                if (domain == null)
                    throw new Exception($"Memory domain '{region.Domain}' not found");

                domain.PokeBytes(writeAddress, data, true);
            });

            return CreateSuccessResult($"Successfully wrote {data.Length} bytes to '{region.Description}' at address 0x{writeAddress:X}");
        }
    }
}
