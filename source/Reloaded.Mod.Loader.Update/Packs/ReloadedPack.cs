namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Represents a pack that contains a collection of mods to be installed.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class ReloadedPack : IConfig
{
    /// <summary>
    /// Name of the pack.
    /// </summary>
    public string Name { get; set; } = String.Empty;
    
    /// <summary>
    /// Readme for the pack, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;

    /// <summary>
    /// List of preview image files belonging to the pack.
    /// May be PNG, JPEG & JXL (JPEG XL).
    /// </summary>
    public List<ReloadedPackImage> ImageFiles { get; set; } = new();
    
    /// <summary>
    /// Items associated with this pack.
    /// </summary>
    public List<ReloadedPackItem> Items { get; set; } = new();
}