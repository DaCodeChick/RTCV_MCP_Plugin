using Newtonsoft.Json;
using System.Collections.Generic;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// Tool definition for MCP
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// Tool name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Tool description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Input schema (JSON Schema)
        /// </summary>
        [JsonProperty("inputSchema")]
        public ToolInputSchema InputSchema { get; set; }
    }

    /// <summary>
    /// Tool input schema (JSON Schema format)
    /// </summary>
    public class ToolInputSchema
    {
        /// <summary>
        /// Schema type (usually "object")
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "object";

        /// <summary>
        /// Properties definition
        /// </summary>
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Required property names
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Required { get; set; }
    }

    /// <summary>
    /// Tool execution result
    /// </summary>
    public class ToolCallResult
    {
        /// <summary>
        /// Result content (array of content items)
        /// </summary>
        [JsonProperty("content")]
        public List<ToolContent> Content { get; set; } = new List<ToolContent>();

        /// <summary>
        /// Whether the tool execution failed
        /// </summary>
        [JsonProperty("isError", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsError { get; set; }
    }

    /// <summary>
    /// Tool content item
    /// </summary>
    public class ToolContent
    {
        /// <summary>
        /// Content type (e.g., "text")
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Text content (when type is "text")
        /// </summary>
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
    }

    /// <summary>
    /// Tool call request parameters
    /// </summary>
    public class ToolCallParams
    {
        /// <summary>
        /// Tool name to call
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Tool arguments
        /// </summary>
        [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Arguments { get; set; }
    }

    /// <summary>
    /// Tools list result
    /// </summary>
    public class ToolsListResult
    {
        /// <summary>
        /// Available tools
        /// </summary>
        [JsonProperty("tools")]
        public List<ToolDefinition> Tools { get; set; } = new List<ToolDefinition>();
    }
}
