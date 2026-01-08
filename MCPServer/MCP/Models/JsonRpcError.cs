using Newtonsoft.Json;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// JSON-RPC 2.0 Error object
    /// </summary>
    public class JsonRpcError
    {
        /// <summary>
        /// Error code (standard JSON-RPC error codes)
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Additional error data (optional)
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }
    }

    /// <summary>
    /// JSON-RPC 2.0 Error response message
    /// </summary>
    public class JsonRpcErrorResponse
    {
        /// <summary>
        /// JSON-RPC version (always "2.0")
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Request identifier (must match the request, or null if id was invalid)
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// Error details
        /// </summary>
        [JsonProperty("error")]
        public JsonRpcError Error { get; set; }
    }

    /// <summary>
    /// Standard JSON-RPC 2.0 error codes
    /// </summary>
    public static class JsonRpcErrorCodes
    {
        /// <summary>
        /// Invalid JSON was received by the server
        /// </summary>
        public const int ParseError = -32700;

        /// <summary>
        /// The JSON sent is not a valid Request object
        /// </summary>
        public const int InvalidRequest = -32600;

        /// <summary>
        /// The method does not exist / is not available
        /// </summary>
        public const int MethodNotFound = -32601;

        /// <summary>
        /// Invalid method parameter(s)
        /// </summary>
        public const int InvalidParams = -32602;

        /// <summary>
        /// Internal JSON-RPC error
        /// </summary>
        public const int InternalError = -32603;

        /// <summary>
        /// Server error range start (application-defined)
        /// </summary>
        public const int ServerErrorStart = -32000;

        /// <summary>
        /// Server error range end (application-defined)
        /// </summary>
        public const int ServerErrorEnd = -32099;
    }
}
