using System;
using System.IO;
using System.Threading.Tasks;
using RTCV.Plugins.MCPServer.Config;
using RTCV.Plugins.MCPServer.Logging;
using RTCV.Plugins.MCPServer.MCP.Models;
using RTCV.Plugins.MCPServer.MCP.Tools;
using RTCV.Plugins.MCPServer.MCP.Transport;

namespace RTCV.Plugins.MCPServer.MCP
{
    /// <summary>
    /// Core MCP server implementation
    /// </summary>
    public class McpServer : IDisposable
    {
        private const string ProtocolVersion = "2024-11-05";
        private const string ServerName = "RTCV MCP Server";
        private const string ServerVersion = "1.0.0";

        private readonly ServerConfig config;
        private readonly Logger logger;
        private readonly JsonRpcHandler rpcHandler;
        private readonly ToolRegistry toolRegistry;
        private readonly MemoryRegionManager regionManager;

        private ITransport transport;
        private ServerState state;
        private bool disposed;
        private readonly object stateLock = new object();

        public event EventHandler<ServerStateChangedEventArgs> StateChanged;

        public ServerState State
        {
            get
            {
                lock (stateLock)
                {
                    return state;
                }
            }
            private set
            {
                lock (stateLock)
                {
                    if (state != value)
                    {
                        var oldState = state;
                        state = value;
                        OnStateChanged(oldState, value);
                    }
                }
            }
        }

        public McpServer(ServerConfig config, Logger logger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            this.rpcHandler = new JsonRpcHandler();
            this.toolRegistry = new ToolRegistry(config);
            this.state = ServerState.Stopped;

            // Initialize memory region manager
            string pluginDir = Path.GetDirectoryName(typeof(McpServer).Assembly.Location);
            string regionsPath = Path.Combine(pluginDir, "..", "..", "Plugins", "MCPServer", "MemoryRegions");
            this.regionManager = new MemoryRegionManager(Path.GetFullPath(regionsPath));

            // Apply config values to static properties
            EmulationTarget.MaxFileNameLength = config.Server.MaxFileNameLength;

            // Register all tool handlers
            RegisterToolHandlers();

            logger.LogInfo("MCP Server instance created");
        }

        /// <summary>
        /// Register all RTCV tool handlers with the tool registry
        /// </summary>
        private void RegisterToolHandlers()
        {
            // Blast tools
            toolRegistry.RegisterTool(new BlastGenerateHandler());
            toolRegistry.RegisterTool(new BlastToggleHandler());
            toolRegistry.RegisterTool(new BlastSetIntensityHandler());

            // Status tools
            toolRegistry.RegisterTool(new GetStatusHandler());
            toolRegistry.RegisterTool(new GetEmulationTargetHandler());
            toolRegistry.RegisterTool(new MemoryDomainsListHandler());

            // Engine tools
            toolRegistry.RegisterTool(new EngineGetConfigHandler());
            toolRegistry.RegisterTool(new EngineSetConfigHandler());

            // Savestate tools
            toolRegistry.RegisterTool(new SavestateCreateHandler());
            toolRegistry.RegisterTool(new SavestateLoadHandler());

            // Stockpile tools
            toolRegistry.RegisterTool(new StockpileAddHandler());
            toolRegistry.RegisterTool(new StockpileApplyHandler());

            // Memory tools (disabled by default in config)
            toolRegistry.RegisterTool(new MemoryReadHandler());
            toolRegistry.RegisterTool(new MemoryWriteHandler());

            // Memory region annotation tools
            toolRegistry.RegisterTool(new AddMemoryRegionTool(regionManager));
            toolRegistry.RegisterTool(new ListMemoryRegionsTool(regionManager));
            toolRegistry.RegisterTool(new GetMemoryRegionTool(regionManager));
            toolRegistry.RegisterTool(new UpdateMemoryRegionTool(regionManager));
            toolRegistry.RegisterTool(new RemoveMemoryRegionTool(regionManager));
            toolRegistry.RegisterTool(new ReadMemoryRegionTool(regionManager));
            toolRegistry.RegisterTool(new WriteMemoryRegionTool(regionManager));

            logger.LogInfo("Registered all tool handlers");
        }

        /// <summary>
        /// Start the MCP server
        /// </summary>
        public void Start()
        {
            if (State != ServerState.Stopped)
            {
                throw new InvalidOperationException($"Cannot start server in {State} state");
            }

            State = ServerState.Starting;
            logger.LogInfo("Starting MCP server...");

            try
            {
                // Create transport based on configuration
                if (config.Server.EnableHttp && config.Server.EnableStdio)
                {
                    // Both transports enabled - prioritize HTTP
                    logger.LogWarning("Both HTTP and stdio transports enabled, using HTTP");
                    transport = new HttpTransport(
                        config.Server.Address, 
                        config.Server.Port,
                        config.Server.MaxRequestSizeBytes,
                        config.Server.ShutdownTimeoutMs);
                    logger.LogInfo($"Using HTTP transport on {config.Server.Address}:{config.Server.Port}");
                }
                else if (config.Server.EnableHttp)
                {
                    transport = new HttpTransport(
                        config.Server.Address, 
                        config.Server.Port,
                        config.Server.MaxRequestSizeBytes,
                        config.Server.ShutdownTimeoutMs);
                    logger.LogInfo($"Using HTTP transport on {config.Server.Address}:{config.Server.Port}");
                }
                else if (config.Server.EnableStdio)
                {
                    transport = new StdioTransport(config.Server.ShutdownTimeoutMs);
                    logger.LogInfo("Using stdio transport");
                }
                else
                {
                    throw new InvalidOperationException("No transport enabled in configuration");
                }

                // Subscribe to transport events
                transport.MessageReceived += OnTransportMessageReceived;
                transport.Error += OnTransportError;

                // Start transport
                transport.Start();
                logger.LogInfo("Transport started");

                // Server is now running
                State = ServerState.Running;
                logger.LogInfo("MCP server started successfully");
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to start MCP server", ex);
                State = ServerState.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stop the MCP server
        /// </summary>
        public void Stop()
        {
            if (State == ServerState.Stopped || State == ServerState.Stopping)
            {
                return;
            }

            State = ServerState.Stopping;
            logger.LogInfo("Stopping MCP server...");

            try
            {
                // Stop transport
                if (transport != null)
                {
                    transport.MessageReceived -= OnTransportMessageReceived;
                    transport.Error -= OnTransportError;
                    transport.Stop();
                    transport.Dispose();
                    transport = null;
                }

                State = ServerState.Stopped;
                logger.LogInfo("MCP server stopped");
            }
            catch (Exception ex)
            {
                logger.LogError("Error stopping MCP server", ex);
                State = ServerState.Stopped;
            }
        }

        /// <summary>
        /// Handle message received from transport
        /// </summary>
        private async void OnTransportMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                logger.LogVerbose($"Received message: {e.Message}");

                // Parse JSON-RPC request
                JsonRpcRequest request;
                try
                {
                    request = rpcHandler.ParseRequest(e.Message);
                }
                catch (JsonRpcException ex)
                {
                    // Parse error - send error response if possible
                    logger.LogError($"JSON-RPC parse error: {ex.Message}");
                    SendError(null, ex.Code, ex.Message, ex.Data);
                    return;
                }

                // Check if it's a notification (no response expected)
                bool isNotification = rpcHandler.IsNotification(request);

                // Dispatch request
                try
                {
                    await HandleRequestAsync(request, isNotification);
                }
                catch (JsonRpcException ex)
                {
                    if (!isNotification)
                    {
                        SendError(request.Id, ex.Code, ex.Message, ex.Data);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error handling request '{request.Method}'", ex);
                    if (!isNotification)
                    {
                        SendError(request.Id, JsonRpcErrorCodes.InternalError, 
                            "Internal server error", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Unhandled error in message handler", ex);
            }
        }

        /// <summary>
        /// Handle transport error
        /// </summary>
        private void OnTransportError(object sender, TransportErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                logger.LogError(e.Message, e.Exception);
            }
            else
            {
                logger.LogInfo(e.Message);
            }
        }

        /// <summary>
        /// Handle a JSON-RPC request
        /// </summary>
        private async Task HandleRequestAsync(JsonRpcRequest request, bool isNotification)
        {
            logger.LogNormal($"Handling method: {request.Method}");

            switch (request.Method)
            {
                case "initialize":
                    HandleInitialize(request);
                    break;

                case "notifications/initialized":
                    // Client signals initialization complete (notification only)
                    logger.LogInfo("Client initialization complete");
                    break;

                case "tools/list":
                    HandleToolsList(request);
                    break;

                case "tools/call":
                    await HandleToolCallAsync(request);
                    break;

                default:
                    if (!isNotification)
                    {
                        throw new JsonRpcException(JsonRpcErrorCodes.MethodNotFound,
                            $"Method '{request.Method}' not found");
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle initialize request
        /// </summary>
        private void HandleInitialize(JsonRpcRequest request)
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

            // Send response
            SendResponse(request.Id, result);
            logger.LogInfo("Initialize response sent");
        }

        /// <summary>
        /// Handle tools/list request
        /// </summary>
        private void HandleToolsList(JsonRpcRequest request)
        {
            logger.LogInfo("Handling tools/list request");

            // Get enabled tools from registry
            var tools = toolRegistry.GetEnabledTools();

            var result = new ToolsListResult
            {
                Tools = tools
            };

            SendResponse(request.Id, result);
            logger.LogInfo($"Tools list sent ({tools.Count} tools)");
        }

        /// <summary>
        /// Handle tools/call request
        /// </summary>
        private async Task HandleToolCallAsync(JsonRpcRequest request)
        {
            var toolParams = rpcHandler.GetParams<ToolCallParams>(request);

            if (toolParams == null || string.IsNullOrEmpty(toolParams.Name))
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, 
                    "Tool name is required");
            }

            logger.LogInfo($"Handling tool call: {toolParams.Name}");

            // Invoke tool asynchronously
            var result = await toolRegistry.InvokeToolAsync(toolParams.Name, toolParams.Arguments);

            SendResponse(request.Id, result);
            logger.LogInfo($"Tool call result sent for: {toolParams.Name}");
        }

        /// <summary>
        /// Send a success response
        /// </summary>
        private void SendResponse(object id, object result)
        {
            string response = rpcHandler.BuildResponse(id, result);
            logger.LogVerbose($"Sending response: {response}");
            transport?.SendMessage(response);
        }

        /// <summary>
        /// Send an error response
        /// </summary>
        private void SendError(object id, int code, string message, object data = null)
        {
            string error = rpcHandler.BuildError(id, code, message, data);
            logger.LogVerbose($"Sending error: {error}");
            transport?.SendMessage(error);
        }

        /// <summary>
        /// Raise state changed event
        /// </summary>
        private void OnStateChanged(ServerState oldState, ServerState newState)
        {
            logger.LogInfo($"State changed: {oldState} -> {newState}");
            StateChanged?.Invoke(this, new ServerStateChangedEventArgs(oldState, newState));
        }

        /// <summary>
        /// Get the tool registry
        /// </summary>
        public ToolRegistry ToolRegistry => toolRegistry;

        public void Dispose()
        {
            if (!disposed)
            {
                Stop();
                disposed = true;
            }
        }
    }

    /// <summary>
    /// Event arguments for state changed event
    /// </summary>
    public class ServerStateChangedEventArgs : EventArgs
    {
        public ServerState OldState { get; }
        public ServerState NewState { get; }

        public ServerStateChangedEventArgs(ServerState oldState, ServerState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
