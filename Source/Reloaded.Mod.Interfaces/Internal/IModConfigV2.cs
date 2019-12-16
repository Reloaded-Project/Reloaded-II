using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModConfigV2 : IModConfigV1
    {
        /// <summary>
        /// A collection of optional <see cref="ModId"/>(s) of other modifications required by this mod.
        /// Specifying the optional ModIds in this field allows you to use controllers and plugins from said mods.
        /// </summary>
        string[] OptionalDependencies { get; set; }
    }
}
