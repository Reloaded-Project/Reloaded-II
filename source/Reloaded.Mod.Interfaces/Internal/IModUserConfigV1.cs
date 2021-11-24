using System.Collections.Generic;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModUserConfigV1
    {
        /// <summary>
        /// Data stored by plugins. Maps a unique string key to arbitrary data.
        /// </summary>
        public Dictionary<string, object> PluginData { get; set; }

        /// <summary>
        /// True if the mod is a universal mod and should be available for every application by default.
        /// </summary>
        public bool IsUniversalMod { get; set; }
    }
}