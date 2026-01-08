using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// MCP server information
    /// </summary>
    public class McpServerInfo
    {
        /// <summary>
        /// Server name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Server version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
    }

    /// <summary>
    /// MCP client information
    /// </summary>
    public class McpClientInfo
    {
        /// <summary>
        /// Client name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Client version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
    }

    /// <summary>
    /// MCP server capabilities
    /// </summary>
    public class McpCapabilities
    {
        /// <summary>
        /// Tool capabilities
        /// </summary>
        [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
        public ToolsCapability Tools { get; set; }
    }

    /// <summary>
    /// Tool capability information
    /// </summary>
    public class ToolsCapability
    {
        /// <summary>
        /// Whether the tool list can change dynamically
        /// </summary>
        [JsonProperty("listChanged", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ListChanged { get; set; }
    }

    /// <summary>
    /// MCP initialize request parameters
    /// </summary>
    public class McpInitializeParams
    {
        /// <summary>
        /// Protocol version
        /// </summary>
        [JsonProperty("protocolVersion")]
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// Client capabilities
        /// </summary>
        [JsonProperty("capabilities")]
        public object Capabilities { get; set; }

        /// <summary>
        /// Client information
        /// </summary>
        [JsonProperty("clientInfo")]
        public McpClientInfo ClientInfo { get; set; }
    }

    /// <summary>
    /// MCP initialize response result
    /// </summary>
    public class McpInitializeResult
    {
        /// <summary>
        /// Protocol version
        /// </summary>
        [JsonProperty("protocolVersion")]
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// Server capabilities
        /// </summary>
        [JsonProperty("capabilities")]
        public McpCapabilities Capabilities { get; set; }

        /// <summary>
        /// Server information
        /// </summary>
        [JsonProperty("serverInfo")]
        public McpServerInfo ServerInfo { get; set; }
    }
}
