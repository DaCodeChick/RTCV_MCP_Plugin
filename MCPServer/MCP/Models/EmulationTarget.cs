using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RTCV.NetCore;
using RTCV.CorruptCore;
using RTCV.Plugins.MCPServer.Helpers;

namespace RTCV.Plugins.MCPServer.MCP.Models
{
    /// <summary>
    /// Represents the current emulation target (system + game + core)
    /// </summary>
    public class EmulationTarget
    {
        /// <summary>
        /// Maximum filename length (configurable, default 200)
        /// </summary>
        internal static int MaxFileNameLength { get; set; } = 200;

        /// <summary>
        /// System name (e.g., "NES", "SNES", "Genesis")
        /// </summary>
        [JsonProperty("system")]
        public string System { get; set; }

        /// <summary>
        /// Game name (e.g., "Super Mario Bros.", "The Legend of Zelda")
        /// </summary>
        [JsonProperty("game_name")]
        public string GameName { get; set; }

        /// <summary>
        /// Core/emulator name (e.g., "Nestopia", "Mesen", "Genesis Plus GX")
        /// </summary>
        [JsonProperty("core")]
        public string Core { get; set; }

        /// <summary>
        /// Vanguard implementation name
        /// </summary>
        [JsonProperty("vanguard_name")]
        public string VanguardName { get; set; }

        /// <summary>
        /// Whether an emulator is currently attached
        /// </summary>
        [JsonProperty("attached")]
        public bool Attached { get; set; }

        /// <summary>
        /// Get a sanitized filename safe identifier for this target
        /// </summary>
        [JsonIgnore]
        public string FilenameSafeId
        {
            get
            {
                // Create a unique identifier from system, game, and core
                string combined = $"{System}_{GameName}_{Core}";
                
                // Remove invalid filename characters
                string safe = string.Join("_", combined.Split(Path.GetInvalidFileNameChars()));
                
                // Remove extra whitespace and replace spaces with underscores
                safe = Regex.Replace(safe, @"\s+", "_");
                
                // Limit length to prevent filesystem issues
                if (safe.Length > MaxFileNameLength)
                {
                    safe = safe.Substring(0, MaxFileNameLength);
                }
                
                return safe;
            }
        }

        /// <summary>
        /// Get a human-readable display name for this target
        /// </summary>
        [JsonIgnore]
        public string DisplayName => $"{GameName} ({System} - {Core})";

        /// <summary>
        /// Get the current emulation target from RTCV
        /// </summary>
        public static EmulationTarget GetCurrent()
        {
            return RtcvThreadHelper.ExecuteOnFormThread(() =>
            {
                var target = new EmulationTarget();

                if (AllSpec.VanguardSpec != null)
                {
                    target.System = AllSpec.VanguardSpec[VSPEC.SYSTEM] as string ?? "Unknown";
                    target.GameName = AllSpec.VanguardSpec[VSPEC.GAMENAME] as string ?? "None";
                    target.Core = AllSpec.VanguardSpec[VSPEC.SYSTEMCORE] as string ?? "Unknown";
                    target.VanguardName = AllSpec.VanguardSpec[VSPEC.NAME] as string ?? "Unknown";
                    target.Attached = RtcCore.Attached;
                }
                else
                {
                    target.System = "Unknown";
                    target.GameName = "None";
                    target.Core = "Unknown";
                    target.VanguardName = "Unknown";
                    target.Attached = false;
                }

                return target;
            });
        }

        /// <summary>
        /// Validate that a valid emulation target is loaded
        /// </summary>
        public bool IsValid()
        {
            return Attached && 
                   !string.IsNullOrEmpty(GameName) && 
                   GameName != "None" &&
                   !string.IsNullOrEmpty(System) &&
                   System != "Unknown";
        }
    }
}
