namespace Reloaded.Mod.Loader.Update.Index.Structures;

/// <summary>
/// Exposes a collection of packages for serialization.
/// </summary>
public struct PackageList
{
    // TODO: Restore constructor with non-nullable guarantee on .NET 7 upgrade.

    /// <summary>
    /// The packages contained in this list.
    /// </summary>
    public List<Package> Packages { get; set; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public static PackageList Create()
    {
        return new PackageList()
        {
            Packages = new List<Package>()
        };
    }

    /// <summary>
    /// Removes the info that is not needed for dependency resolution.
    /// </summary>
    public void RemoveNonDependencyInfo()
    {
        foreach (var package in Packages)
        {
            package.RemoveNonDependencyInfo();
        }
    }
}