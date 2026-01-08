namespace RTCV.Plugins.MCPServer.MCP.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using RTCV.Plugins.MCPServer.Helpers;
    using RTCV.Plugins.MCPServer.MCP.Models;

    /// <summary>
    /// Tool handler for reading memory values.
    /// WARNING: This tool is disabled by default for safety.
    /// </summary>
    public class MemoryReadHandler : ToolHandlerBase
    {
        public override string Name => "memory_read";
        public override string Description => "Read memory values from a specific domain and address";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["domain"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Memory domain name (e.g., 'System Bus', 'Main RAM')"
                },
                ["address"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Memory address to read from"
                },
                ["length"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Number of bytes to read (1-1024)"
                }
            },
            Required = new List<string> { "domain", "address", "length" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "domain");
            ValidateRequiredArgument(arguments, "address");
            ValidateRequiredArgument(arguments, "length");

            string domainName = GetArgument<string>(arguments, "domain");
            long address = Convert.ToInt64(arguments["address"]);
            int length = Convert.ToInt32(arguments["length"]);

            if (length < 1 || length > 1024)
            {
                return CreateErrorResult("Length must be between 1 and 1024 bytes");
            }

            var data = RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                if (AllSpec.VanguardSpec == null)
                {
                    throw new InvalidOperationException("No emulator connected");
                }

                var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                {
                    throw new InvalidOperationException("No memory domains available");
                }

                var domain = domains.FirstOrDefault(d => d.Name == domainName);
                if (domain == null)
                {
                    throw new ArgumentException($"Memory domain '{domainName}' not found");
                }

                if (address < 0 || address + length > domain.Size)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Address range 0x{address:X}-0x{address + length:X} is outside domain bounds (0x0-0x{domain.Size:X})"
                    );
                }

                byte[] bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = domain.PeekByte(address + i);
                }

                return bytes;
            });

            // Format output as hex dump
            string hexString = BitConverter.ToString(data).Replace("-", " ");
            return CreateSuccessResult($"Read {length} bytes from {domainName}:0x{address:X}\n\nHex: {hexString}");
        }
    }

    /// <summary>
    /// Tool handler for writing memory values.
    /// WARNING: This tool is disabled by default for safety.
    /// </summary>
    public class MemoryWriteHandler : ToolHandlerBase
    {
        public override string Name => "memory_write";
        public override string Description => "Write memory values to a specific domain and address";

        public override ToolInputSchema InputSchema => new ToolInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, object>
            {
                ["domain"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Memory domain name (e.g., 'System Bus', 'Main RAM')"
                },
                ["address"] = new Dictionary<string, object>
                {
                    ["type"] = "number",
                    ["description"] = "Memory address to write to"
                },
                ["data"] = new Dictionary<string, object>
                {
                    ["type"] = "array",
                    ["description"] = "Array of byte values to write (0-255)",
                    ["items"] = new Dictionary<string, object>
                    {
                        ["type"] = "number"
                    }
                }
            },
            Required = new List<string> { "domain", "address", "data" }
        };

        protected override ToolCallResult ExecuteCore(Dictionary<string, object> arguments)
        {
            ValidateRequiredArgument(arguments, "domain");
            ValidateRequiredArgument(arguments, "address");
            ValidateRequiredArgument(arguments, "data");

            string domainName = GetArgument<string>(arguments, "domain");
            long address = Convert.ToInt64(arguments["address"]);
            
            // Parse data array
            if (!(arguments["data"] is IEnumerable<object> dataObjects))
            {
                return CreateErrorResult("Data must be an array of numbers");
            }

            byte[] data = dataObjects.Select(o => Convert.ToByte(o)).ToArray();

            if (data.Length < 1 || data.Length > 1024)
            {
                return CreateErrorResult("Data length must be between 1 and 1024 bytes");
            }

            RtcvThreadHelper.ExecuteOnEmuThread(() =>
            {
                if (AllSpec.VanguardSpec == null)
                {
                    throw new InvalidOperationException("No emulator connected");
                }

                var memoryDomainsObj = AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES];
                if (memoryDomainsObj == null || !(memoryDomainsObj is MemoryDomainProxy[] domains))
                {
                    throw new InvalidOperationException("No memory domains available");
                }

                var domain = domains.FirstOrDefault(d => d.Name == domainName);
                if (domain == null)
                {
                    throw new ArgumentException($"Memory domain '{domainName}' not found");
                }

                if (domain.ReadOnly)
                {
                    throw new InvalidOperationException($"Memory domain '{domainName}' is not writable");
                }

                if (address < 0 || address + data.Length > domain.Size)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Address range 0x{address:X}-0x{address + data.Length:X} is outside domain bounds (0x0-0x{domain.Size:X})"
                    );
                }

                for (int i = 0; i < data.Length; i++)
                {
                    domain.PokeByte(address + i, data[i]);
                }
            });

            string hexString = BitConverter.ToString(data).Replace("-", " ");
            return CreateSuccessResult($"Wrote {data.Length} bytes to {domainName}:0x{address:X}\n\nHex: {hexString}");
        }
    }
}
