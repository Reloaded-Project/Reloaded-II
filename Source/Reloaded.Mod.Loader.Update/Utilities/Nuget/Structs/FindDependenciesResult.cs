using System.Collections.Generic;
using NuGet.Protocol.Core.Types;

namespace Reloaded.Mod.Loader.Update.Utilities.Nuget
{
    /// <summary>
    /// Contains the results of a dependency search.
    /// </summary>
    public struct NugetFindDependenciesResult
    {
        /// <summary>
        /// All found dependencies.
        /// </summary>
        public HashSet<IPackageSearchMetadata> Dependencies { get; private set; }
        
        /// <summary>
        /// Mod IDs of packages not found.
        /// </summary>
        public HashSet<string> PackagesNotFound { get; private set; }

        public NugetFindDependenciesResult(HashSet<IPackageSearchMetadata> dependencies, HashSet<string> packagesNotFound)
        {
            Dependencies = dependencies;
            PackagesNotFound = packagesNotFound;
        }
    }
}
