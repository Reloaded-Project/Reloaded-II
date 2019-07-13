using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.IO.Structs.Dependencies
{
    /// <summary>
    /// Represents a collection of mod dependencies.
    /// </summary>
    public class ModDependencySet
    {
        public HashSet<ModConfig>  Configurations          { get; } = new HashSet<ModConfig>();
        public HashSet<string>     MissingConfigurations   { get; } = new HashSet<string>();

        /* Default */
        public ModDependencySet() { }

        /// <summary>
        /// Merges a set of sets together into one large set.
        /// </summary>
        public ModDependencySet(IEnumerable<ModDependencySet> sets)
        {
            foreach (var dependencySet in sets)
            {
                Configurations.UnionWith(dependencySet.Configurations);
                MissingConfigurations.UnionWith(dependencySet.MissingConfigurations);
            }
        }
    }
}
