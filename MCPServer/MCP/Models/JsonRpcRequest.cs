using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// JSON-RPC 2.0 Request message
    /// </summary>
    public class JsonRpcRequest
    {
        /// <summary>
        /// JSON-RPC version (always "2.0")
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Request identifier (can be string, number, or null for notifications)
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// Method name to invoke
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; set; }

        /// <summary>
        /// Method parameters (optional)
        /// </summary>
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public object Params { get; set; }
    }
}
