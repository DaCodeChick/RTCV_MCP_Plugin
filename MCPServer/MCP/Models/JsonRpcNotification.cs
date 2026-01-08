using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// JSON-RPC 2.0 Notification (request without id, no response expected)
    /// </summary>
    public class JsonRpcNotification
    {
        /// <summary>
        /// JSON-RPC version (always "2.0")
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Method name
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
