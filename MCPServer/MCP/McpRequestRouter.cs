using System;
using System.Threading.Tasks;
using RTCV.Plugins.MCPServer.Logging;
using RTCV.Plugins.MCPServer.MCP.Models;
using RTCV.Plugins.MCPServer.MCP.Tools;

namespace RTCV.Plugins.MCPServer.MCP
{
    /// <summary>
    /// Routes incoming MCP JSON-RPC requests to appropriate handlers
    /// </summary>
    internal class McpRequestRouter
    {
        private readonly ToolRegistry toolRegistry;
        private readonly McpProtocolHandler protocolHandler;
        private readonly Logger logger;

        public McpRequestRouter(ToolRegistry toolRegistry, McpProtocolHandler protocolHandler, Logger logger)
        {
            this.toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            this.protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Route a JSON-RPC request to the appropriate handler
        /// </summary>
        public async Task<object> RouteRequestAsync(JsonRpcRequest request, bool isNotification)
        {
            logger.LogNormal($"Routing method: {request.Method}");

            switch (request.Method)
            {
                case "initialize":
                    return protocolHandler.HandleInitialize(request);

                case "notifications/initialized":
                    // Client signals initialization complete (notification only)
                    logger.LogInfo("Client initialization complete");
                    return null;

                case "tools/list":
                    return protocolHandler.HandleToolsList(request);

                case "tools/call":
                    return await HandleToolCallAsync(request);

                default:
                    if (!isNotification)
                    {
                        throw new JsonRpcException(JsonRpcErrorCodes.MethodNotFound,
                            $"Method '{request.Method}' not found");
                    }
                    return null;
            }
        }

        /// <summary>
        /// Handle tools/call request
        /// </summary>
        private async Task<ToolCallResult> HandleToolCallAsync(JsonRpcRequest request)
        {
            var rpcHandler = new JsonRpcHandler();
            var toolParams = rpcHandler.GetParams<ToolCallParams>(request);

            if (toolParams == null || string.IsNullOrEmpty(toolParams.Name))
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, 
                    "Tool name is required");
            }

            logger.LogInfo($"Handling tool call: {toolParams.Name}");

            // Invoke tool asynchronously
            var result = await toolRegistry.InvokeToolAsync(toolParams.Name, toolParams.Arguments);

            logger.LogInfo($"Tool call completed: {toolParams.Name}");
            return result;
        }
    }
}
