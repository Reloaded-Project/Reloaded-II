namespace Reloaded.Mod.Loader.Update.Index.Structures;

/// <summary>
/// Exposes a collection of packages for serialization.
/// </summary>
public struct PackageList
{
    /// <summary>
    /// The packages contained in this list.
    /// </summary>
    public List<Package> Packages { get; set; } = new();

    public PackageList() { }
}