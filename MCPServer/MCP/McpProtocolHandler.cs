using RTCV.Plugins.MCPServer.Logging;
using RTCV.Plugins.MCPServer.MCP.Models;
using RTCV.Plugins.MCPServer.MCP.Tools;

namespace RTCV.Plugins.MCPServer.MCP
{
    /// <summary>
    /// Handles MCP protocol-specific requests (initialize, tools/list)
    /// </summary>
    internal class McpProtocolHandler
    {
        private const string ProtocolVersion = "2024-11-05";
        private const string ServerName = "RTCV MCP Server";
        private const string ServerVersion = "1.0.0";

        private readonly ToolRegistry toolRegistry;
        private readonly Logger logger;
        private readonly JsonRpcHandler rpcHandler;

        public McpProtocolHandler(ToolRegistry toolRegistry, Logger logger)
        {
            this.toolRegistry = toolRegistry;
            this.logger = logger;
            this.rpcHandler = new JsonRpcHandler();
        }

        /// <summary>
        /// Handle initialize request
        /// </summary>
        public McpInitializeResult HandleInitialize(JsonRpcRequest request)
        {
            logger.LogInfo("Handling initialize request");

            // Parse parameters
            var initParams = rpcHandler.GetParams<McpInitializeParams>(request);

            // Log client info
            if (initParams?.ClientInfo != null)
            {
                logger.LogInfo($"Client: {initParams.ClientInfo.Name} v{initParams.ClientInfo.Version}");
            }

            // Build initialize result
            var result = new McpInitializeResult
            {
                ProtocolVersion = ProtocolVersion,
                ServerInfo = new McpServerInfo
                {
                    Name = ServerName,
                    Version = ServerVersion
                },
                Capabilities = new McpCapabilities
                {
                    Tools = new ToolsCapability
                    {
                        ListChanged = false
                    }
                }
            };

            logger.LogInfo("Initialize response prepared");
            return result;
        }

        /// <summary>
        /// Handle tools/list request
        /// </summary>
        public ToolsListResult HandleToolsList(JsonRpcRequest request)
        {
            logger.LogInfo("Handling tools/list request");

            // Get enabled tools from registry
            var tools = toolRegistry.GetEnabledTools();

            var result = new ToolsListResult
            {
                Tools = tools
            };

            logger.LogInfo($"Tools list prepared ({tools.Count} tools)");
            return result;
        }
    }
}
