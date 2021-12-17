using System.Collections.Generic;
using NuGet.Protocol.Core.Types;

namespace Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

/// <summary>
/// Contains the result of a NuGet dependency search given a search from multiple package sources.
/// </summary>
public struct AggregateFindDependenciesResult
{
    /// <summary>
    /// All found dependencies for the current package.
    /// </summary>
    public HashSet<NugetTuple<IPackageSearchMetadata>> Dependencies { get; private set; }

    /// <summary>
    /// Mod IDs of packages not found.
    /// </summary>
    public HashSet<string> PackagesNotFound { get; private set; }

    /// <summary/>
    public AggregateFindDependenciesResult(HashSet<NugetTuple<IPackageSearchMetadata>> dependencies, HashSet<string> packagesNotFound)
    {
        Dependencies = dependencies;
        PackagesNotFound = packagesNotFound;
    }
}