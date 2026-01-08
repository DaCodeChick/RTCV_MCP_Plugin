using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RTCV.Plugins.MCPServer.Config;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// Interface for tool handlers
    /// </summary>
    public interface IToolHandler
    {
        /// <summary>
        /// Tool name (unique identifier)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Tool description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Tool input schema
        /// </summary>
        ToolInputSchema InputSchema { get; }

        /// <summary>
        /// Execute the tool asynchronously
        /// </summary>
        /// <param name="arguments">Tool arguments</param>
        /// <returns>Tool execution result</returns>
        Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments);
    }

    /// <summary>
    /// Registry for managing MCP tools
    /// </summary>
    public class ToolRegistry
    {
        private readonly Dictionary<string, IToolHandler> tools;
        private readonly ServerConfig config;
        private readonly object lockObject = new object();

        public ToolRegistry(ServerConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.tools = new Dictionary<string, IToolHandler>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Register a tool handler
        /// </summary>
        /// <param name="tool">Tool handler to register</param>
        public void RegisterTool(IToolHandler tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            if (string.IsNullOrEmpty(tool.Name))
            {
                throw new ArgumentException("Tool name cannot be null or empty");
            }

            lock (lockObject)
            {
                if (tools.ContainsKey(tool.Name))
                {
                    throw new InvalidOperationException($"Tool '{tool.Name}' is already registered");
                }

                tools[tool.Name] = tool;
            }
        }

        /// <summary>
        /// Get all enabled tool definitions
        /// </summary>
        /// <returns>List of enabled tool definitions</returns>
        public List<ToolDefinition> GetEnabledTools()
        {
            lock (lockObject)
            {
                var enabledTools = new List<ToolDefinition>();

                foreach (var tool in tools.Values)
                {
                    // Check if tool is enabled in config
                    if (config.Tools.TryGetValue(tool.Name, out var toolConfig) && toolConfig.Enabled)
                    {
                        enabledTools.Add(new ToolDefinition
                        {
                            Name = tool.Name,
                            Description = tool.Description,
                            InputSchema = tool.InputSchema
                        });
                    }
                }

                return enabledTools;
            }
        }

        /// <summary>
        /// Check if a tool exists and is enabled
        /// </summary>
        /// <param name="toolName">Tool name</param>
        /// <returns>True if tool exists and is enabled</returns>
        public bool IsToolEnabled(string toolName)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                return false;
            }

            lock (lockObject)
            {
                if (!tools.ContainsKey(toolName))
                {
                    return false;
                }

                if (config.Tools.TryGetValue(toolName, out var toolConfig))
                {
                    return toolConfig.Enabled;
                }

                return false;
            }
        }

        /// <summary>
        /// Invoke a tool asynchronously
        /// </summary>
        /// <param name="toolName">Tool name</param>
        /// <param name="arguments">Tool arguments</param>
        /// <returns>Tool execution result</returns>
        public async Task<ToolCallResult> InvokeToolAsync(string toolName, Dictionary<string, object> arguments)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                return CreateErrorResult("Tool name is required");
            }

            IToolHandler tool;
            lock (lockObject)
            {
                // Check if tool exists
                if (!tools.TryGetValue(toolName, out tool))
                {
                    return CreateErrorResult($"Tool '{toolName}' not found");
                }

                // Check if tool is enabled
                if (!config.Tools.TryGetValue(toolName, out var toolConfig) || !toolConfig.Enabled)
                {
                    return CreateErrorResult($"Tool '{toolName}' is disabled");
                }
            }

            try
            {
                // Execute tool
                var result = await tool.ExecuteAsync(arguments ?? new Dictionary<string, object>());
                return result;
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"Tool execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get count of registered tools
        /// </summary>
        public int ToolCount
        {
            get
            {
                lock (lockObject)
                {
                    return tools.Count;
                }
            }
        }

        /// <summary>
        /// Create an error result
        /// </summary>
        private ToolCallResult CreateErrorResult(string message)
        {
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = message
                    }
                }
            };
        }
    }
}
