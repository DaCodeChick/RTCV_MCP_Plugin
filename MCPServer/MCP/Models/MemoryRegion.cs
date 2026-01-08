using System;
using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// Represents an annotated memory region with semantic description
    /// </summary>
    public class MemoryRegion
    {
        /// <summary>
        /// Unique identifier for this region
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Human-readable name/description (e.g., "Mario sprite palette")
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Memory domain (e.g., "WRAM", "SRAM", "ROM")
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Start address of the region
        /// </summary>
        [JsonProperty("address")]
        public long Address { get; set; }

        /// <summary>
        /// Size of the region in bytes
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>
        /// Optional tags for categorization (e.g., ["sprite", "palette", "mario"])
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Optional notes about the region
        /// </summary>
        [JsonProperty("notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Data type hint (e.g., "palette", "sprite", "text", "integer", "float")
        /// </summary>
        [JsonProperty("data_type")]
        public string DataType { get; set; }

        /// <summary>
        /// Timestamp when region was created
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when region was last updated
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public MemoryRegion()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Tags = new string[0];
        }
    }
}
