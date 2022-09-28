namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Represents an individual item contained in a Reloaded pack.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class ReloadedPackItem
{
    /// <summary>
    /// Name of the mod represented by this item.
    /// [Shown in UI]
    /// </summary>
    public string Name { get; set; } = String.Empty;
 
    /// <summary>
    /// ID of the mod contained.
    /// [Shown in UI]
    /// </summary>
    public string ModId { get; set; } = String.Empty;
    
    /// <summary>
    /// Readme for this mod, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;

    /// <summary>
    /// List of preview image files belonging to this item.
    /// May be PNG, JPEG and JXL (JPEG XL).
    /// </summary>
    public List<ReloadedPackImage> ImageFiles { get; set; } = new();

    /// <summary>
    /// Copied from <see cref="ModConfig.PluginData"/>. Contains info on how to download the mod.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; } = new();
}