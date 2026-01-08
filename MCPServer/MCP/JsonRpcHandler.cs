using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP
{
    /// <summary>
    /// Handles parsing and building JSON-RPC 2.0 messages
    /// </summary>
    public class JsonRpcHandler
    {
        private readonly JsonSerializerSettings serializerSettings;

        public JsonRpcHandler()
        {
            // Configure JSON serialization
            serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        /// <summary>
        /// Parse a JSON-RPC request from a JSON string
        /// </summary>
        /// <param name="json">JSON string to parse</param>
        /// <returns>Parsed JsonRpcRequest</returns>
        /// <exception cref="JsonRpcException">Thrown if parsing fails</exception>
        public JsonRpcRequest ParseRequest(string json)
        {
            try
            {
                // Parse JSON
                var request = JsonConvert.DeserializeObject<JsonRpcRequest>(json);

                if (request == null)
                {
                    throw new JsonRpcException(JsonRpcErrorCodes.InvalidRequest, "Request is null");
                }

                // Validate JSON-RPC version
                if (request.JsonRpc != "2.0")
                {
                    throw new JsonRpcException(JsonRpcErrorCodes.InvalidRequest, 
                        "Invalid JSON-RPC version (must be '2.0')");
                }

                // Validate method exists
                if (string.IsNullOrEmpty(request.Method))
                {
                    throw new JsonRpcException(JsonRpcErrorCodes.InvalidRequest, 
                        "Method is required");
                }

                return request;
            }
            catch (JsonException ex)
            {
                throw new JsonRpcException(JsonRpcErrorCodes.ParseError, 
                    "Invalid JSON", ex);
            }
        }

        /// <summary>
        /// Build a successful JSON-RPC response
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="result">Result object</param>
        /// <returns>JSON string</returns>
        public string BuildResponse(object id, object result)
        {
            var response = new JsonRpcResponse
            {
                Id = id,
                Result = result
            };

            return JsonConvert.SerializeObject(response, serializerSettings);
        }

        /// <summary>
        /// Build a JSON-RPC error response
        /// </summary>
        /// <param name="id">Request ID (can be null if request ID was invalid)</param>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        /// <param name="data">Additional error data (optional)</param>
        /// <returns>JSON string</returns>
        public string BuildError(object id, int code, string message, object data = null)
        {
            var errorResponse = new JsonRpcErrorResponse
            {
                Id = id,
                Error = new JsonRpcError
                {
                    Code = code,
                    Message = message,
                    Data = data
                }
            };

            return JsonConvert.SerializeObject(errorResponse, serializerSettings);
        }

        /// <summary>
        /// Build a JSON-RPC notification (server-initiated message)
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="parameters">Method parameters (optional)</param>
        /// <returns>JSON string</returns>
        public string BuildNotification(string method, object parameters = null)
        {
            var notification = new JsonRpcNotification
            {
                Method = method,
                Params = parameters
            };

            return JsonConvert.SerializeObject(notification, serializerSettings);
        }

        /// <summary>
        /// Check if a request is a notification (no response expected)
        /// </summary>
        /// <param name="request">Request to check</param>
        /// <returns>True if notification</returns>
        public bool IsNotification(JsonRpcRequest request)
        {
            return request.Id == null;
        }

        /// <summary>
        /// Extract typed parameters from a request
        /// </summary>
        /// <typeparam name="T">Expected parameter type</typeparam>
        /// <param name="request">Request containing parameters</param>
        /// <returns>Typed parameters</returns>
        /// <exception cref="JsonRpcException">Thrown if parameter conversion fails</exception>
        public T GetParams<T>(JsonRpcRequest request)
        {
            try
            {
                if (request.Params == null)
                {
                    return default(T);
                }

                // If params is already a JObject or JArray, convert it
                if (request.Params is JToken jToken)
                {
                    return jToken.ToObject<T>();
                }

                // Otherwise, serialize and deserialize (handles anonymous objects)
                string json = JsonConvert.SerializeObject(request.Params);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, 
                    "Failed to parse parameters", ex);
            }
        }
    }

    /// <summary>
    /// Exception thrown for JSON-RPC errors
    /// </summary>
    public class JsonRpcException : Exception
    {
        public int Code { get; }
        public new object Data { get; }

        public JsonRpcException(int code, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Code = code;
        }

        public JsonRpcException(int code, string message, object data)
            : base(message)
        {
            Code = code;
            Data = data;
        }
    }
}
