using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RTCV.Plugins.MCPServer.MCP.Models;

namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    /// <summary>
    /// Base class for tool handlers with common error handling
    /// </summary>
    public abstract class ToolHandlerBase : IToolHandler
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract ToolInputSchema InputSchema { get; }

        /// <summary>
        /// Execute the tool with automatic error handling
        /// </summary>
        public async Task<ToolCallResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ToolLogger.Log($"Executing {Name}...");
                    var result = ExecuteCore(arguments ?? new Dictionary<string, object>());
                    ToolLogger.Log($"{Name} completed successfully");
                    return result;
                }
                catch (Exception ex)
                {
                    ToolLogger.LogError($"Error executing {Name}: {ex.Message}");
                    return CreateErrorResult($"Error executing {Name}: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Core execution logic - implement this in derived classes
        /// </summary>
        protected abstract ToolCallResult ExecuteCore(Dictionary<string, object> arguments);

        /// <summary>
        /// Create a success result with text content
        /// </summary>
        protected ToolCallResult CreateSuccessResult(string text)
        {
            return new ToolCallResult
            {
                Content = new List<ToolContent>
                {
                    new ToolContent { Type = "text", Text = text }
                },
                IsError = false
            };
        }

        /// <summary>
        /// Create an error result with text content
        /// </summary>
        protected ToolCallResult CreateErrorResult(string text)
        {
            return new ToolCallResult
            {
                Content = new List<ToolContent>
                {
                    new ToolContent { Type = "text", Text = text }
                },
                IsError = true
            };
        }

        /// <summary>
        /// Validate that a required argument exists
        /// </summary>
        protected void ValidateRequiredArgument(Dictionary<string, object> arguments, string key)
        {
            if (!arguments.ContainsKey(key) || arguments[key] == null)
            {
                throw new ArgumentException($"Missing required argument: {key}");
            }
        }

        /// <summary>
        /// Get argument value with type conversion
        /// </summary>
        protected T GetArgument<T>(Dictionary<string, object> arguments, string key, T defaultValue = default(T))
        {
            if (!arguments.ContainsKey(key) || arguments[key] == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(arguments[key], typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
