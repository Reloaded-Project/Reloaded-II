using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModLoaderV2
    {
        /// <summary>
        /// Retrieves the directory of a mod with a specific Mod ID.
        /// </summary>
        /// <param name="modId">The mod id of the mod.</param>
        /// <returns>The directory containing the mod.</returns>
        string GetDirectoryForModId(string modId);
    }
}
