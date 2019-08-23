using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Loader.Mods.Structs
{
    public class ModAssemblyMetadata
    {
        /// <summary>
        /// List of all types exported by the mod.
        /// </summary>
        public Type[] Exports { get; set; }

        /// <summary>
        /// True if this type can be unloaded, else false.
        /// </summary>
        public bool   IsUnloadable { get; set; }

        public ModAssemblyMetadata(Type[] exports, bool isUnloadable)
        {
            Exports = exports;
            IsUnloadable = isUnloadable;
        }
    }
}
