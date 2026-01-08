using System;
using System.Collections.Generic;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// Base class for memory region-related tools
    /// </summary>
    public abstract class MemoryRegionToolBase : ToolHandlerBase
    {
        protected readonly MemoryRegionManager RegionManager;

        protected MemoryRegionToolBase(MemoryRegionManager regionManager)
        {
            RegionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        }

        /// <summary>
        /// Ensure a valid emulation target is loaded and load its regions
        /// </summary>
        protected EmulationTarget EnsureValidTarget()
        {
            var target = EmulationTarget.GetCurrent();
            if (!target.IsValid())
            {
                throw new InvalidOperationException("No valid emulation target loaded. Please load a game first.");
            }

            RegionManager.LoadRegions(target);
            return target;
        }

        /// <summary>
        /// Parse tags array from arguments
        /// </summary>
        protected string[] GetTags(Dictionary<string, object> arguments)
        {
            if (!arguments.ContainsKey("tags") || arguments["tags"] == null)
                return null;
            
            if (arguments["tags"] is object[] arr)
                return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(arr, t => t?.ToString()));
            
            return null;
        }
    }
}
