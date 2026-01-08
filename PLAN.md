# RTCV MCP Server Plugin - Complete Implementation Plan

## Project Overview

Creating an RTCV (Real-Time Corruptor Vanguard) plugin that implements a Model Context Protocol (MCP) server, exposing RTCV's corruption capabilities via JSON-RPC 2.0 over stdio transport.

---

## Executive Summary

### What We're Building
- **Name**: RTCV MCP Server Plugin
- **Purpose**: Expose RTCV corruption operations to MCP clients (like Claude Desktop)
- **Protocol**: JSON-RPC 2.0 (MCP specification)
- **Transport**: stdio (standard input/output) - HTTP in future phase
- **Framework**: .NET Framework 4.7.1
- **Plugin System**: MEF (Managed Extensibility Framework)
- **RTCV Version**: 5.1
- **IDE**: Visual Studio 2022

### Key Features
- 13 MCP tools exposing RTCV operations
- Settings UI accessible from Tools menu
- JSON-based configuration with persistence
- File-based logging with configurable levels
- Safety defaults (dangerous tools disabled)
- Auto-start capability (disabled by default)

---

## Technical Specifications

### Configuration Defaults

```json
{
  "version": "1.0",
  "server": {
    "autoStart": false,
    "address": "127.0.0.1",
    "port": 8080,
    "enableHttp": false,
    "enableStdio": true
  },
  "logging": {
    "enabled": true,
    "path": "Plugins/MCPServer/Logs/mcp.log",
    "level": "Normal"
  },
  "tools": {
    "blast_generate": { "enabled": true, "requireConfirmation": false },
    "blast_toggle": { "enabled": true, "requireConfirmation": false },
    "blast_set_intensity": { "enabled": true, "requireConfirmation": false },
    "memory_domains_list": { "enabled": true, "requireConfirmation": false },
    "get_status": { "enabled": true, "requireConfirmation": false },
    "engine_get_config": { "enabled": true, "requireConfirmation": false },
    "engine_set_config": { "enabled": true, "requireConfirmation": false },
    "savestate_create": { "enabled": true, "requireConfirmation": false },
    "savestate_load": { "enabled": true, "requireConfirmation": false },
    "stockpile_add": { "enabled": true, "requireConfirmation": false },
    "stockpile_apply": { "enabled": true, "requireConfirmation": false },
    "memory_read": { "enabled": false, "requireConfirmation": true },
    "memory_write": { "enabled": false, "requireConfirmation": true }
  }
}
```

**Safety Note**: `memory_read` and `memory_write` are disabled by default to prevent unsafe operations.

### Libraries We Can Use (Already in RTCV)
- **Newtonsoft.Json** - JSON serialization/deserialization
- **System.IO** - stdio streams
- **System.Threading** - Background processing
- **System.ComponentModel.Composition** (MEF) - Plugin system
- **RTCV DLLs**: PluginHost, Common, CorruptCore, NetCore, UI

---

## Complete Project Structure

```
RTCV_Plugin_MCPServer/
├── MCPServer/
│   ├── MCPServer.csproj                        # ⚠️ Update DLL paths before build
│   ├── MCPServer.sln                           # Visual Studio solution
│   ├── DLL-REFERENCES.md                       # 📝 Guide to required DLLs
│   │
│   ├── PluginCore.cs                           # MEF entry point, plugin lifecycle
│   │
│   ├── Config/
│   │   ├── ServerConfig.cs                     # Configuration data model
│   │   ├── ToolConfig.cs                       # Individual tool settings
│   │   └── ConfigManager.cs                    # Load/save JSON config
│   │
│   ├── MCP/
│   │   ├── McpServer.cs                        # Core MCP server, lifecycle manager
│   │   ├── ServerState.cs                      # Server state enum
│   │   ├── JsonRpcHandler.cs                   # JSON-RPC 2.0 message parser/builder
│   │   │
│   │   ├── Transport/
│   │   │   ├── ITransport.cs                   # Transport abstraction
│   │   │   └── StdioTransport.cs               # stdio implementation
│   │   │
│   │   ├── Models/
│   │   │   ├── JsonRpcRequest.cs               # JSON-RPC request structure
│   │   │   ├── JsonRpcResponse.cs              # JSON-RPC response structure
│   │   │   ├── JsonRpcError.cs                 # JSON-RPC error codes/messages
│   │   │   ├── JsonRpcNotification.cs          # JSON-RPC notification
│   │   │   ├── McpInitializeParams.cs          # MCP initialize request params
│   │   │   ├── McpInitializeResult.cs          # MCP initialize response
│   │   │   ├── McpServerInfo.cs                # Server metadata
│   │   │   ├── McpCapabilities.cs              # Server capabilities
│   │   │   ├── ToolDefinition.cs               # Tool metadata
│   │   │   ├── ToolInput.cs                    # Tool input schema
│   │   │   └── ToolCallResult.cs               # Tool execution result
│   │   │
│   │   └── Tools/
│   │       ├── IToolHandler.cs                 # Tool handler interface
│   │       ├── ToolRegistry.cs                 # Register and dispatch tools
│   │       ├── BlastTools.cs                   # blast_generate, blast_toggle, blast_set_intensity
│   │       ├── StatusTools.cs                  # get_status, memory_domains_list
│   │       ├── EngineTools.cs                  # engine_get_config, engine_set_config
│   │       ├── StateTools.cs                   # savestate_create, savestate_load
│   │       ├── StockpileTools.cs               # stockpile_add, stockpile_apply
│   │       └── MemoryTools.cs                  # memory_read, memory_write
│   │
│   ├── UI/
│   │   ├── MCPServerForm.cs                    # Settings UI form
│   │   └── MCPServerForm.Designer.cs           # Form designer code
│   │
│   └── Logging/
│       └── Logger.cs                           # File-based logging with levels
│
├── Config/
│   └── config.json.example                     # Example configuration file
│
├── Docs/
│   ├── README.md                               # Main documentation
│   ├── BUILDING.md                             # Build instructions (DLL setup)
│   ├── TOOLS.md                                # Tool reference
│   ├── CONFIGURATION.md                        # Config guide
│   ├── TESTING.md                              # Testing with Bizhawk
│   └── TROUBLESHOOTING.md                      # Common issues
│
└── LICENSE                                     # MIT License
```

---

## Detailed Component Design

### 1. PluginCore.cs - MEF Entry Point

**Responsibilities:**
- MEF plugin entry point
- Initialize configuration manager
- Register UI in Tools menu
- Start/stop MCP server based on config
- Handle plugin lifecycle

**Key Structure:**
```csharp
[Export(typeof(IPlugin))]
public class PluginCore : IPlugin, IDisposable
{
    public string Name => "MCP Server";
    public string Description => "Model Context Protocol server for RTCV";
    public string Author => "Your Name";
    public string Version => "1.0.0";
    public RTCSide SupportedSide => RTCSide.Server; // RTC side only
    
    private McpServer mcpServer;
    private MCPServerForm settingsForm;
    private ConfigManager configManager;
    
    public bool Start(RTCSide side)
    {
        // Load configuration
        // Register form in Tools menu
        // Auto-start server if configured
        // Return success/failure
    }
    
    public bool StopPlugin()
    {
        // Stop MCP server if running
        // Save configuration
        // Cleanup resources
    }
    
    public void Dispose()
    {
        // Final cleanup
    }
}
```

**Integration Points:**
- `S.GET<OpenToolsForm>().RegisterTool()` - Register settings form in Tools menu
- `ConfigManager.LoadConfig()` - Load settings from JSON file
- `McpServer.Start(config)` - Start server if auto-start enabled

---

### 2. Configuration System

#### ServerConfig.cs (Data Model)
```csharp
public class ServerConfig
{
    public string Version { get; set; } = "1.0";
    public ServerSettings Server { get; set; } = new ServerSettings();
    public LoggingSettings Logging { get; set; } = new LoggingSettings();
    public Dictionary<string, ToolConfig> Tools { get; set; } = new Dictionary<string, ToolConfig>();
}

public class ServerSettings
{
    public bool AutoStart { get; set; } = false;
    public string Address { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8080;
    public bool EnableHttp { get; set; } = false;
    public bool EnableStdio { get; set; } = true;
}

public class LoggingSettings
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "Plugins/MCPServer/Logs/mcp.log";
    public LogLevel Level { get; set; } = LogLevel.Normal;
}

public enum LogLevel { Minimal, Normal, Verbose }
```

#### ToolConfig.cs
```csharp
public class ToolConfig
{
    public bool Enabled { get; set; } = true;
    public bool RequireConfirmation { get; set; } = false;
}
```

#### ConfigManager.cs
```csharp
public class ConfigManager
{
    private const string ConfigDirectory = "Plugins/MCPServer/Config";
    private const string ConfigFileName = "config.json";
    
    public ServerConfig LoadConfig()
    {
        // Load from JSON file
        // If doesn't exist, create with defaults
        // Handle errors gracefully
    }
    
    public void SaveConfig(ServerConfig config)
    {
        // Ensure directory exists
        // Serialize to JSON
        // Write to file
        // Handle errors
    }
    
    public ServerConfig GetDefaultConfig()
    {
        // Return config with safety defaults
        // memory_read/write disabled
    }
}
```

---

### 3. MCP Server Core

#### McpServer.cs
**Responsibilities:**
- Manage server lifecycle (start/stop)
- Initialize transport layer
- Handle MCP initialization handshake
- Route messages to appropriate handlers
- Emit notifications

**State Machine:**
```
Stopped → Starting → Running → Stopping → Stopped
```

**Key Methods:**
```csharp
public class McpServer : IDisposable
{
    private ServerConfig config;
    private ITransport transport;
    private JsonRpcHandler rpcHandler;
    private ToolRegistry toolRegistry;
    private Logger logger;
    private ServerState state;
    private Thread serverThread;
    
    public event EventHandler<ServerStateChangedEventArgs> StateChanged;
    
    public McpServer(ServerConfig config)
    {
        // Initialize components
        // Set up tool registry
        // Create logger
    }
    
    public void Start()
    {
        // Transition to Starting
        // Create transport (stdio)
        // Start background thread
        // Begin listening for messages
        // Transition to Running
    }
    
    public void Stop()
    {
        // Transition to Stopping
        // Stop accepting new requests
        // Cleanup transport
        // Join thread
        // Transition to Stopped
    }
    
    private void MessageLoop()
    {
        // Run on background thread
        // Read messages from transport
        // Parse JSON-RPC
        // Handle initialize handshake
        // Dispatch tool calls
        // Send responses
        // Log activity
    }
    
    private void HandleInitialize(JsonRpcRequest request)
    {
        // MCP initialization handshake
        // Negotiate protocol version
        // Return server capabilities
        // List available tools
    }
    
    private void HandleToolsList(JsonRpcRequest request)
    {
        // Return list of enabled tools
        // Include schemas for each tool
    }
    
    private void HandleToolCall(JsonRpcRequest request)
    {
        // Validate tool is enabled
        // Parse arguments
        // Invoke tool handler
        // Return result or error
    }
}
```

**Thread Safety:**
- Background thread for message loop
- Use `SyncObjectSingleton.FormExecute()` for RTCV UI operations
- Lock on state transitions

---

### 4. JSON-RPC Handler

#### JsonRpcHandler.cs
**Responsibilities:**
- Parse incoming JSON-RPC 2.0 messages
- Validate message structure
- Build response/error messages
- Handle batch requests (optional)

**Key Methods:**
```csharp
public class JsonRpcHandler
{
    public JsonRpcRequest ParseRequest(string json)
    {
        // Deserialize using Newtonsoft.Json
        // Validate structure (jsonrpc: "2.0", method, id)
        // Return typed request or throw
    }
    
    public string BuildResponse(object id, object result)
    {
        // Create JsonRpcResponse
        // Serialize to JSON string
        // Return formatted message
    }
    
    public string BuildError(object id, int code, string message, object data = null)
    {
        // Create JsonRpcError
        // Serialize to JSON string
        // Standard error codes (-32700 to -32603)
    }
    
    public string BuildNotification(string method, object parameters)
    {
        // Create notification (no id field)
        // For server-initiated events
    }
}
```

**Error Codes:**
- `-32700` Parse error
- `-32600` Invalid Request
- `-32601` Method not found
- `-32602` Invalid params
- `-32603` Internal error
- `-32000 to -32099` Server error range

---

### 5. Transport Layer

#### ITransport.cs (Interface)
```csharp
public interface ITransport : IDisposable
{
    event EventHandler<MessageReceivedEventArgs> MessageReceived;
    event EventHandler<TransportErrorEventArgs> Error;
    
    void Start();
    void Stop();
    void SendMessage(string message);
    bool IsConnected { get; }
}
```

#### StdioTransport.cs
**Responsibilities:**
- Read from `Console.OpenStandardInput()`
- Write to `Console.OpenStandardOutput()`
- Handle message framing (newline-delimited JSON)
- Emit events for received messages

**Implementation Notes:**
```csharp
public class StdioTransport : ITransport
{
    private Stream stdin;
    private Stream stdout;
    private StreamReader reader;
    private StreamWriter writer;
    private bool isRunning;
    
    public void Start()
    {
        // Open standard streams
        stdin = Console.OpenStandardInput();
        stdout = Console.OpenStandardOutput();
        reader = new StreamReader(stdin);
        writer = new StreamWriter(stdout) { AutoFlush = true };
        isRunning = true;
        
        // Start reading in loop
    }
    
    private void ReadLoop()
    {
        while (isRunning)
        {
            string line = reader.ReadLine();
            if (line != null)
            {
                OnMessageReceived(line);
            }
        }
    }
    
    public void SendMessage(string message)
    {
        // Write JSON message + newline
        writer.WriteLine(message);
    }
    
    public void Stop()
    {
        isRunning = false;
        // Cleanup streams
    }
}
```

**⚠️ Critical Note:** stdio transport must NOT use `Console.Write()`/`Console.WriteLine()` for anything except JSON-RPC messages. All logging must go to file or stderr.

---

### 6. Tool System

#### IToolHandler.cs (Interface)
```csharp
public interface IToolHandler
{
    string Name { get; }
    string Description { get; }
    ToolInput InputSchema { get; }
    
    Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments);
}
```

#### ToolRegistry.cs
```csharp
public class ToolRegistry
{
    private Dictionary<string, IToolHandler> tools;
    private ServerConfig config;
    
    public void RegisterTool(IToolHandler tool)
    {
        // Add to registry
    }
    
    public List<ToolDefinition> GetEnabledTools()
    {
        // Return definitions for tools enabled in config
    }
    
    public async Task<ToolCallResult> InvokeToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        // Check if tool exists
        // Check if tool is enabled
        // Invoke handler
        // Return result or error
    }
}
```

---

### 7. Tool Implementations

All 13 tools to implement:

#### 1. **blast_generate** (BlastTools.cs)
- Generate and execute a corruption blast
- Input: `intensity` (1-100), `errorDelay` (milliseconds)
- Uses: `BlastLayer.GenerateBlast()`, `CorruptCore`

#### 2. **blast_toggle** (BlastTools.cs)
- Enable/disable auto-corrupt
- Input: `enabled` (boolean)
- Uses: `CorruptCore.AutoCorrupt`

#### 3. **blast_set_intensity** (BlastTools.cs)
- Adjust blast intensity and error delay
- Input: `intensity`, `errorDelay`
- Uses: `CorruptCore` settings

#### 4. **memory_domains_list** (StatusTools.cs)
- Query available memory domains
- No input
- Uses: `AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES]`

#### 5. **get_status** (StatusTools.cs)
- Get current corruption status
- Returns: auto-corrupt state, intensity, active engine, connected emulator
- Uses: `CorruptCore`, `AllSpec`

#### 6. **engine_get_config** (EngineTools.cs)
- Get current corruption engine settings
- Uses: `S.GET<CorruptionEngineForm>()`

#### 7. **engine_set_config** (EngineTools.cs)
- Update corruption engine parameters
- Input: engine-specific settings
- Uses: Engine configuration APIs

#### 8. **savestate_create** (StateTools.cs)
- Create savestate via Glitch Harvester
- Input: `name` (optional)
- Returns: savestate ID/path

#### 9. **savestate_load** (StateTools.cs)
- Load savestate by ID/path
- Input: `id` or `path`
- Returns: success/failure

#### 10. **stockpile_add** (StockpileTools.cs)
- Add current corruption to stockpile
- Input: `name`, `notes` (optional)
- Uses: `StashKey` API

#### 11. **stockpile_apply** (StockpileTools.cs)
- Apply stockpile item by ID
- Input: `id`
- Returns: success/failure

#### 12. **memory_read** (MemoryTools.cs) - ⚠️ DISABLED BY DEFAULT
- Read memory from specified domain/address
- Input: `domain`, `address`, `length`
- Returns: byte array

#### 13. **memory_write** (MemoryTools.cs) - ⚠️ DISABLED BY DEFAULT
- Write memory to specified domain/address
- Input: `domain`, `address`, `data` (byte array)
- Returns: success/failure

**Example Tool Implementation:**
```csharp
public class BlastGenerateHandler : IToolHandler
{
    public string Name => "blast_generate";
    public string Description => "Generate and execute a corruption blast";
    public ToolInput InputSchema => new ToolInput
    {
        Type = "object",
        Properties = new Dictionary<string, object>
        {
            ["intensity"] = new { type = "integer", description = "Blast intensity (1-100)", minimum = 1, maximum = 100 },
            ["errorDelay"] = new { type = "integer", description = "Error delay in milliseconds", minimum = 0 }
        }
    };
    
    public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
    {
        // Parse arguments
        // Use SyncObjectSingleton.FormExecute() for UI thread
        // Call BlastLayer.GenerateBlast()
        // Execute blast via CorruptCore
        // Return result with blast details
    }
}
```

---

### 8. UI Settings Form

#### MCPServerForm.cs Layout

```
┌─────────────────────────────────────────────────┐
│  MCP Server Settings                      [X]   │
├─────────────────────────────────────────────────┤
│                                                 │
│  Server Settings                                │
│  ┌──────────────────────────────────────────┐  │
│  │ [✓] Auto-start server on plugin load    │  │
│  │                                          │  │
│  │ Address: [127.0.0.1            ]        │  │
│  │ Port:    [8080   ]                      │  │
│  │                                          │  │
│  │ [_] Enable HTTP transport (coming soon) │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  Logging Settings                               │
│  ┌──────────────────────────────────────────┐  │
│  │ [✓] Enable logging                       │  │
│  │                                          │  │
│  │ Log Path: [Plugins/MCPServer/.../]      │  │
│  │           [Browse...]                    │  │
│  │                                          │  │
│  │ Log Level: [Normal ▼]                   │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  Tool Configuration                             │
│  ┌──────────────────────────────────────────┐  │
│  │ [✓] blast_generate                       │  │
│  │ [✓] blast_toggle                         │  │
│  │ [✓] blast_set_intensity                  │  │
│  │ [✓] memory_domains_list                  │  │
│  │ [✓] get_status                           │  │
│  │ [✓] engine_get_config                    │  │
│  │ [✓] engine_set_config                    │  │
│  │ [✓] savestate_create                     │  │
│  │ [✓] savestate_load                       │  │
│  │ [✓] stockpile_add                        │  │
│  │ [✓] stockpile_apply                      │  │
│  │ [_] memory_read (⚠ unsafe)               │  │
│  │ [_] memory_write (⚠ unsafe)              │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  Server Status: [●] Running  [Start] [Stop]    │
│                                                 │
│  [Save Settings]  [Reset to Defaults] [Close]  │
└─────────────────────────────────────────────────┘
```

**Key Features:**
- Real-time server status indicator
- Start/Stop buttons (gray out based on state)
- Browse button for log path selection
- Checkboxes for individual tool enable/disable
- Visual indicator (⚠) for unsafe tools
- Save applies settings and persists to config.json
- Reset to Defaults restores safe configuration

---

### 9. Logging System

#### Logger.cs
```csharp
public enum LogLevel { Minimal, Normal, Verbose }

public class Logger : IDisposable
{
    private string logPath;
    private LogLevel level;
    private bool enabled;
    private StreamWriter writer;
    private object lockObject = new object();
    
    public Logger(LoggingSettings settings)
    {
        // Initialize from settings
        // Create log directory if needed
        // Open log file for append
    }
    
    public void LogMinimal(string message)
    {
        if (enabled && level >= LogLevel.Minimal)
            WriteLog("MINIMAL", message);
    }
    
    public void LogNormal(string message)
    {
        if (enabled && level >= LogLevel.Normal)
            WriteLog("NORMAL", message);
    }
    
    public void LogVerbose(string message)
    {
        if (enabled && level >= LogLevel.Verbose)
            WriteLog("VERBOSE", message);
    }
    
    public void LogError(string message, Exception ex = null)
    {
        if (enabled)
            WriteLog("ERROR", message + (ex != null ? $"\n{ex}" : ""));
    }
    
    private void WriteLog(string level, string message)
    {
        lock (lockObject)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            writer?.WriteLine($"[{timestamp}] [{level}] {message}");
            writer?.Flush();
        }
    }
}
```

**Log Levels:**
- **Minimal**: Errors and critical events only
- **Normal**: Connection events, tool calls, errors
- **Verbose**: All JSON-RPC messages (request/response)

---

## Implementation Phases

### Phase 1: Foundation & Configuration (Days 1-2)
**Goal:** Project setup, configuration system, basic structure

**Tasks:**
1. Create Visual Studio solution and project
2. Add RTCV DLL references (PluginHost, Common, CorruptCore, NetCore, UI)
3. Add Newtonsoft.Json reference (already in RTCV)
4. Implement configuration classes (ServerConfig, ToolConfig, etc.)
5. Implement ConfigManager (load/save JSON)
6. Create default config.json.example
7. Implement Logger class
8. Test configuration load/save independently

**Deliverables:**
- Working project structure
- Configuration system with JSON persistence
- Logger writing to file
- Unit tests for config/logging

---

### Phase 2: JSON-RPC & Transport (Days 3-4)
**Goal:** Core protocol handling and stdio transport

**Tasks:**
1. Define JSON-RPC model classes (Request, Response, Error, etc.)
2. Implement JsonRpcHandler (parse/build messages)
3. Implement ITransport interface
4. Implement StdioTransport (stdin/stdout handling)
5. Create simple test harness to send/receive JSON-RPC messages
6. Test message framing and error handling
7. Verify no stdout contamination (logging to file only)

**Deliverables:**
- Working JSON-RPC parser/serializer
- stdio transport reading/writing messages
- Test suite for protocol handling

---

### Phase 3: MCP Server Core (Days 5-7)
**Goal:** MCP server lifecycle, initialization handshake, tool registry

**Tasks:**
1. Implement ServerState enum and state machine
2. Implement McpServer class (start/stop, message loop)
3. Implement MCP initialization handshake
   - Initialize request/response
   - Capability negotiation
   - Protocol version check
4. Implement tools/list handler
5. Create ToolRegistry class
6. Create IToolHandler interface
7. Implement basic tool dispatcher
8. Test initialization sequence with MCP Inspector
9. Verify server can list tools

**Deliverables:**
- Working MCP server core
- Successful initialization handshake
- Tool listing functionality
- Integration test with MCP Inspector

---

### Phase 4: Tool Implementations - Core Set (Days 8-10)
**Goal:** Implement essential RTCV tools

**Tasks:**
1. Implement BlastTools (generate, toggle, set_intensity)
   - Map to CorruptCore APIs
   - Handle thread synchronization with SyncObjectSingleton
2. Implement StatusTools (get_status, memory_domains_list)
   - Query AllSpec and VanguardSpec
3. Test each tool individually
4. Test tool enable/disable from config
5. Handle errors gracefully (RTCV API failures)

**Deliverables:**
- 5 working core tools
- Tools integrated with RTCV APIs
- Error handling for each tool
- Tool execution tests

---

### Phase 5: Tool Implementations - Extended Set (Days 11-12)
**Goal:** Implement remaining tools

**Tasks:**
1. Implement EngineTools (get_config, set_config)
2. Implement StateTools (savestate_create, savestate_load)
3. Implement StockpileTools (stockpile_add, stockpile_apply)
4. Implement MemoryTools (memory_read, memory_write)
   - Extra validation for safety
5. Test all 13 tools end-to-end
6. Document each tool's input schema and behavior

**Deliverables:**
- All 13 tools implemented
- Complete tool test suite
- Tool documentation

---

### Phase 6: UI & Plugin Integration (Days 13-14)
**Goal:** Settings form and plugin lifecycle

**Tasks:**
1. Implement MCPServerForm UI
   - Design form layout
   - Implement controls (checkboxes, textboxes, buttons)
   - Wire up event handlers
2. Implement PluginCore (MEF entry point)
   - Plugin lifecycle (Start, StopPlugin, Dispose)
   - Register form in Tools menu
   - Handle auto-start
3. Test plugin loading in RTCV
4. Test form opening from Tools menu
5. Test start/stop server from UI
6. Test settings persistence

**Deliverables:**
- Working settings UI form
- Full plugin integration
- Plugin lifecycle working correctly
- Settings persistence working

---

### Phase 7: Testing & Polish (Days 15-16)
**Goal:** End-to-end testing, bug fixes, documentation

**Tasks:**
1. End-to-end integration testing
   - Load plugin in RTCV
   - Configure server from UI
   - Start server
   - Connect MCP client (Claude Desktop or custom)
   - Execute all tools
   - Verify RTCV operations
2. Test error cases
   - Invalid tool arguments
   - RTCV API failures
   - Configuration errors
   - Server stop during operation
3. Performance testing
   - Multiple rapid tool calls
   - Large data transfers (memory_read)
4. Write README documentation
5. Create example configurations
6. Write usage guide for MCP clients

**Deliverables:**
- Fully tested plugin
- Complete documentation
- Example configurations
- Usage guide

---

### Phase 8: HTTP Transport (Future - Days 17+)
**Goal:** Add HTTP/SSE transport for remote clients

**Tasks:**
1. Implement HttpTransport class
   - HTTP POST for client requests
   - Server-Sent Events for server notifications
2. Add authentication (bearer token, API key)
3. Update UI to enable/disable HTTP
4. Test with remote MCP clients
5. Add CORS handling if needed
6. Document remote connection setup

**Deliverables:**
- Working HTTP transport
- Remote client connectivity
- Security features
- Updated documentation

---

## RTCV API Reference

### Key APIs We'll Use

**BlastLayer & Corruption:**
```csharp
// From CorruptCore namespace
BlastLayer.GenerateBlast()
CorruptCore.AutoCorrupt (property)
CorruptCore.Intensity (property)
CorruptCore.ErrorDelay (property)
```

**Memory Domains:**
```csharp
// From AllSpec
AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES]
// Returns list of IMemoryDomain interfaces
```

**Engine Configuration:**
```csharp
// Engine selection
S.GET<CorruptionEngineForm>()
// Get current engine settings
```

**Glitch Harvester (Savestates):**
```csharp
// Savestate operations
StashKey.Create()
StashKey.Apply()
```

**Stockpile:**
```csharp
// Stockpile management
S.GET<StockpileManagerForm>()
// Add/remove items
```

**Thread Synchronization:**
```csharp
// Execute on UI thread
SyncObjectSingleton.FormExecute(() => {
    // RTCV UI operations here
});

// Execute on emulator thread
SyncObjectSingleton.EmuThreadExecute(() => {
    // Emulator operations here
});
```

---

## MCP Protocol Reference

### Initialization Sequence

**1. Client sends initialize:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
      "name": "example-client",
      "version": "1.0.0"
    }
  }
}
```

**2. Server responds:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "protocolVersion": "2024-11-05",
    "capabilities": {
      "tools": {
        "listChanged": false
      }
    },
    "serverInfo": {
      "name": "RTCV MCP Server",
      "version": "1.0.0"
    }
  }
}
```

**3. Client sends initialized notification:**
```json
{
  "jsonrpc": "2.0",
  "method": "notifications/initialized"
}
```

### Tool Discovery

**Client requests tool list:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list"
}
```

**Server responds with tools:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "tools": [
      {
        "name": "blast_generate",
        "description": "Generate and execute a corruption blast",
        "inputSchema": {
          "type": "object",
          "properties": {
            "intensity": {
              "type": "integer",
              "description": "Blast intensity (1-100)",
              "minimum": 1,
              "maximum": 100
            },
            "errorDelay": {
              "type": "integer",
              "description": "Error delay in milliseconds",
              "minimum": 0
            }
          }
        }
      }
    ]
  }
}
```

### Tool Execution

**Client calls tool:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "blast_generate",
    "arguments": {
      "intensity": 50,
      "errorDelay": 100
    }
  }
}
```

**Server responds with result:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Blast generated successfully with 128 corruption units. Intensity: 50, Error Delay: 100ms"
      }
    ]
  }
}
```

**Server responds with error:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "error": {
    "code": -32000,
    "message": "Tool execution failed",
    "data": {
      "reason": "No emulator connected"
    }
  }
}
```

---

## Testing Strategy

### Testing with Bizhawk50X-Vanguard

**Prerequisites:**
1. Bizhawk50X-Vanguard installed (https://github.com/redscientistlabs/Bizhawk50X-Vanguard)
2. A ROM file (suggest Super Mario 64 or similar well-known game)
3. Plugin compiled and placed in `Plugins/` directory

**Test Scenarios:**

#### **Test 1: Basic Corruption**
```
1. Load Bizhawk + ROM
2. Open RTCV
3. Open MCP Server settings from Tools menu
4. Start MCP server
5. Connect Claude Desktop
6. Execute: "Corrupt the game with medium intensity"
   → Should call blast_set_intensity + blast_toggle
7. Verify auto-corrupt starts in RTCV
8. Verify visible corruption in game
```

#### **Test 2: Memory Domains Query**
```
1. With Bizhawk + ROM loaded
2. Execute: "What memory can I corrupt?"
   → Should call memory_domains_list
3. Verify returns: RDRAM, ROM, etc. for N64
4. Compare with RTCV's domain list
```

#### **Test 3: Savestate Management**
```
1. Corrupt game to interesting state
2. Execute: "Save this glitch"
   → Should call savestate_create + stockpile_add
3. Load different state
4. Execute: "Restore that glitch"
   → Should call stockpile_apply
5. Verify corruption restored correctly
```

#### **Test 4: Engine Configuration**
```
1. Execute: "What corruption engine is active?"
   → Should call get_status
2. Execute: "Configure engine parameters"
   → Should call engine_set_config
3. Verify settings applied in RTCV UI
```

### Unit Tests
- **Configuration**: Load, save, defaults, validation
- **JSON-RPC**: Parse requests, build responses, error codes
- **Tool Handlers**: Input validation, execution, error handling
- **Logger**: Log levels, file writing, rotation

### Integration Tests
- **MCP Handshake**: Initialize sequence, capability negotiation
- **Tool Execution**: Each tool with RTCV APIs
- **Transport**: Message send/receive, framing
- **Plugin Lifecycle**: Load, start, stop, unload

### End-to-End Tests
- **Real MCP Client**: Connect Claude Desktop or custom client
- **Full Workflow**: Initialize → List tools → Call tools → Get results
- **Error Scenarios**: Invalid inputs, API failures, disconnections

### Testing Tools
1. **MCP Inspector** (official tool) - Test MCP protocol compliance
2. **Custom test client** - Scripted tool execution
3. **RTCV test environment** - Load plugin in real RTCV instance
4. **Postman/curl** (for HTTP transport later) - Test HTTP endpoints

---

## Risk Assessment & Mitigation

### Risk 1: stdio Conflicts
**Risk:** Multiple threads writing to stdout could corrupt JSON-RPC messages  
**Mitigation:** 
- Dedicated transport thread owns stdout
- All logging goes to file or stderr
- No Console.WriteLine() except in transport

### Risk 2: RTCV API Thread Safety
**Risk:** MCP server runs on background thread, RTCV APIs need UI thread  
**Mitigation:**
- Use `SyncObjectSingleton.FormExecute()` for all RTCV operations
- Test thoroughly for deadlocks
- Implement timeouts on UI thread calls

### Risk 3: Memory Tool Safety
**Risk:** `memory_write` could crash emulator or corrupt save data  
**Mitigation:**
- Disabled by default
- Require explicit enable in config
- Add validation (address bounds, domain exists)
- Log all memory writes

### Risk 4: Configuration Corruption
**Risk:** Invalid config.json could prevent plugin from loading  
**Mitigation:**
- Validate config on load
- Fall back to defaults on error
- Keep backup of last known good config
- Provide config.json.example

### Risk 5: Server Crash During Operation
**Risk:** Unhandled exception in tool could crash MCP server  
**Mitigation:**
- Try-catch around all tool executions
- Return JSON-RPC error instead of throwing
- Log errors for debugging
- Graceful degradation

---

## Development Workflow (Linux → Windows)

### Working Cross-Platform

**✅ What we CAN do now (on Linux):**
- Create complete project structure
- Write all C# code files
- Create config files (JSON, XML)
- Write documentation
- Review and plan architecture

**⚠️ What REQUIRES Windows:**
- Building the project (compilation)
- Testing in RTCV
- Running Visual Studio 2022
- Referencing RTCV DLLs

**Recommended Workflow:**
1. **Planning Phase (Linux)**: Create all files, write code, plan architecture
2. **Build Phase (Windows)**: Boot to Windows, open in VS2022, fix DLL paths, build
3. **Test Phase (Windows)**: Load plugin in RTCV + Bizhawk, test functionality
4. **Iteration**: Report issues back, make fixes on Linux, rebuild on Windows

### DLL Path Setup (When Ready to Build)

You'll need to update the `.csproj` with your actual paths to RTCV DLLs.

**Required DLLs:**
- PluginHost.dll
- Common.dll
- CorruptCore.dll
- NetCore.dll
- UI.dll
- Newtonsoft.Json.dll

**I'll provide:**
- A placeholder `.csproj` with relative paths
- Comments indicating where to update DLL references
- A separate `DLL-REFERENCES.md` file documenting what DLLs are needed

---

## Success Criteria

The plugin will be considered complete when:

✅ **Core Functionality:**
- [ ] Plugin loads successfully in RTCV
- [ ] Settings form accessible from Tools menu
- [ ] Configuration persists correctly to JSON file
- [ ] MCP server starts/stops cleanly
- [ ] All 13 tools implemented and working

✅ **Protocol Compliance:**
- [ ] Passes MCP Inspector validation
- [ ] Successful initialization handshake
- [ ] Tool discovery returns proper schemas
- [ ] Tool execution returns valid results
- [ ] Error responses follow JSON-RPC 2.0 spec

✅ **Integration:**
- [ ] Works with Claude Desktop
- [ ] Can execute RTCV corruption operations
- [ ] Thread-safe integration with RTCV APIs
- [ ] No crashes or hangs during normal operation

✅ **Safety:**
- [ ] Dangerous tools disabled by default
- [ ] Configuration validation prevents invalid settings
- [ ] Error handling prevents crashes
- [ ] Logging provides debugging information

✅ **Documentation:**
- [ ] README with setup instructions
- [ ] Tool documentation with examples
- [ ] Configuration guide
- [ ] Troubleshooting section

---

## Next Steps

### Immediate Actions (When Continuing on Windows)

1. **Set Up Environment**
   - Open Visual Studio 2022
   - Locate RTCV 5.1 DLLs
   - Note paths for project references

2. **Begin Phase 1: Foundation & Configuration**
   - Create Visual Studio solution
   - Set up project with .NET Framework 4.7.1
   - Add RTCV DLL references
   - Implement configuration system
   - Implement logger

3. **Proceed Through Phases**
   - Follow implementation phases 1-7 in order
   - Test after each phase
   - Document any issues or deviations

### Questions to Resolve Before Building

1. **DLL Paths**: Where are your RTCV 5.1 DLLs located?
2. **Output Directory**: Where should the compiled plugin DLL be placed?
3. **Testing Setup**: Do you have Bizhawk50X-Vanguard ready with a test ROM?
4. **MCP Client**: Will you test with Claude Desktop or a custom client?

---

## Additional Resources

### MCP Documentation
- **Specification**: https://modelcontextprotocol.io/specification/2025-11-25
- **Architecture**: https://modelcontextprotocol.io/docs/learn/architecture
- **Build Server Guide**: https://modelcontextprotocol.io/docs/develop/build-server

### RTCV Resources
- **Main Repo**: https://github.com/redscientistlabs/RTCV
- **Bizhawk Vanguard**: https://github.com/redscientistlabs/Bizhawk50X-Vanguard
- **Plugin Examples**:
  - Memory Visualizer: https://github.com/redscientistlabs/RTCV_Plugin_MemoryVisualizer
  - EZBlastButtons: https://github.com/redscientistlabs/RTCV_Plugin_EZBlastButtons
  - MultiEngine: https://github.com/redscientistlabs/RTCV_Plugin_MultiEngine
- **Documentation**: https://corrupt.wiki/

### JSON-RPC 2.0
- **Specification**: https://www.jsonrpc.org/specification

---

## Contact & Support

When you encounter issues during implementation:

1. **Configuration Issues**: Check `Plugins/MCPServer/Logs/mcp.log`
2. **RTCV Integration**: Check RTCV logs in `Logs/` directory
3. **MCP Protocol**: Use MCP Inspector tool for validation
4. **Build Errors**: Check DLL references and .NET Framework version

---

**This plan is comprehensive and ready for implementation. When you boot to Windows, you can:**
1. Create the Visual Studio project
2. Set up DLL references
3. Begin implementing Phase 1
4. Follow the phases in order

**Good luck with the implementation! 🚀**
