using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RTCV.Common;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// Manages memory region annotations with persistence
    /// </summary>
    public class MemoryRegionManager
    {
        private readonly Dictionary<string, MemoryRegion> _regions;
        private readonly string _storageDirectory;
        private readonly object _lock = new object();

        public MemoryRegionManager(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
            _regions = new Dictionary<string, MemoryRegion>();
            
            // Ensure storage directory exists
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        /// <summary>
        /// Load regions for a specific emulation target
        /// </summary>
        public void LoadRegions(EmulationTarget target)
        {
            lock (_lock)
            {
                try
                {
                    string filePath = GetRegionsFilePath(target);
                    if (File.Exists(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        var regions = JsonConvert.DeserializeObject<List<MemoryRegion>>(json);
                        
                        _regions.Clear();
                        foreach (var region in regions)
                        {
                            _regions[region.Id] = region;
                        }
                    }
                    else
                    {
                        _regions.Clear();
                    }
                }
                catch (Exception ex)
                {
                    RTCV.Common.Logging.GlobalLogger.Error(ex, $"Failed to load memory regions for target: {target.DisplayName}");
                    _regions.Clear();
                }
            }
        }

        /// <summary>
        /// Save regions for a specific emulation target
        /// </summary>
        public void SaveRegions(EmulationTarget target)
        {
            lock (_lock)
            {
                try
                {
                    string filePath = GetRegionsFilePath(target);
                    var regions = _regions.Values.ToList();
                    string json = JsonConvert.SerializeObject(regions, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    RTCV.Common.Logging.GlobalLogger.Error(ex, $"Failed to save memory regions for target: {target.DisplayName}");
                }
            }
        }

        /// <summary>
        /// Add a new memory region
        /// </summary>
        public MemoryRegion AddRegion(EmulationTarget target, string description, string domain, long address, long size, 
            string dataType = null, string[] tags = null, string notes = null)
        {
            lock (_lock)
            {
                var region = new MemoryRegion
                {
                    Description = description,
                    Domain = domain,
                    Address = address,
                    Size = size,
                    DataType = dataType,
                    Tags = tags ?? new string[0],
                    Notes = notes
                };

                _regions[region.Id] = region;
                SaveRegions(target);
                
                return region;
            }
        }

        /// <summary>
        /// Get a region by ID
        /// </summary>
        public MemoryRegion GetRegion(string id)
        {
            lock (_lock)
            {
                return _regions.TryGetValue(id, out var region) ? region : null;
            }
        }

        /// <summary>
        /// Get all regions for current target
        /// </summary>
        public List<MemoryRegion> GetAllRegions()
        {
            lock (_lock)
            {
                return _regions.Values.ToList();
            }
        }

        /// <summary>
        /// Search regions by description (case-insensitive partial match)
        /// </summary>
        public List<MemoryRegion> SearchRegions(string query)
        {
            lock (_lock)
            {
                query = query?.ToLower() ?? "";
                return _regions.Values
                    .Where(r => r.Description?.ToLower().Contains(query) == true ||
                               r.Tags?.Any(t => t.ToLower().Contains(query)) == true ||
                               r.Notes?.ToLower().Contains(query) == true)
                    .ToList();
            }
        }

        /// <summary>
        /// Update an existing region
        /// </summary>
        public bool UpdateRegion(EmulationTarget target, string id, string description = null, string domain = null, 
            long? address = null, long? size = null, string dataType = null, 
            string[] tags = null, string notes = null)
        {
            lock (_lock)
            {
                if (!_regions.TryGetValue(id, out var region))
                {
                    return false;
                }

                if (description != null) region.Description = description;
                if (domain != null) region.Domain = domain;
                if (address.HasValue) region.Address = address.Value;
                if (size.HasValue) region.Size = size.Value;
                if (dataType != null) region.DataType = dataType;
                if (tags != null) region.Tags = tags;
                if (notes != null) region.Notes = notes;
                
                region.UpdatedAt = DateTime.UtcNow;
                
                SaveRegions(target);
                return true;
            }
        }

        /// <summary>
        /// Remove a region by ID
        /// </summary>
        public bool RemoveRegion(EmulationTarget target, string id)
        {
            lock (_lock)
            {
                if (!_regions.Remove(id))
                {
                    return false;
                }

                SaveRegions(target);
                return true;
            }
        }

        /// <summary>
        /// Clear all regions for current target
        /// </summary>
        public void ClearRegions(EmulationTarget target)
        {
            lock (_lock)
            {
                _regions.Clear();
                SaveRegions(target);
            }
        }

        private string GetRegionsFilePath(EmulationTarget target)
        {
            return Path.Combine(_storageDirectory, $"regions_{target.FilenameSafeId}.json");
        }
    }
}
