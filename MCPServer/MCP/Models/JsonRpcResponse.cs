using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// JSON-RPC 2.0 Response message (successful)
    /// </summary>
    public class JsonRpcResponse
    {
        /// <summary>
        /// JSON-RPC version (always "2.0")
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Request identifier (must match the request)
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// Result of the method invocation
        /// </summary>
        [JsonProperty("result")]
        public object Result { get; set; }
    }
}
