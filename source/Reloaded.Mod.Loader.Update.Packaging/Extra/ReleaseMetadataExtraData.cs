namespace Reloaded.Mod.Loader.Update.Packaging.Extra;

/// <summary>
/// Defines the extra metadata stored in an individual release.
/// </summary>
public class ReleaseMetadataExtraData
{
    /// <summary>
    /// Id of the mod contained within.
    /// Copy of <see cref="ModConfig.ModId"/>.
    /// </summary>
    public string? ModId { get; set; }

    /// <summary>
    /// Name of the mod contained within.
    /// Copy of <see cref="ModConfig.ModName"/>.
    /// </summary>
    public string? ModName { get; set; }

    /// <summary>
    /// Description of the mod contained within.
    /// Copy of <see cref="ModConfig.ModDescription"/>.
    /// </summary>
    public string? ModDescription { get; set; }

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    public string? Changelog { get; set; }

    /// <summary>
    /// Contains the readme for the package, encoded as markdown.
    /// </summary>
    public string? Readme { get; set; }
}