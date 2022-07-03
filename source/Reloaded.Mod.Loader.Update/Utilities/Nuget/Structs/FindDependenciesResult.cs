namespace Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

/// <summary>
/// Contains the results of a dependency search.
/// </summary>
public struct FindDependenciesResult
{
    /// <summary>
    /// All found dependencies for the current package.
    /// </summary>
    public HashSet<IPackageSearchMetadata> Dependencies { get; private set; }
        
    /// <summary>
    /// Mod IDs of packages not found.
    /// </summary>
    public HashSet<string> PackagesNotFound { get; private set; }

    /// <summary/>
    public FindDependenciesResult(HashSet<IPackageSearchMetadata> dependencies, HashSet<string> packagesNotFound)
    {
        Dependencies = dependencies;
        PackagesNotFound = packagesNotFound;
    }
}