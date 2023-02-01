namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Represents an individual mod update to be performed.
/// </summary>
public class ModUpdate : ObservableObject
{
    /// <summary>
    /// True if the mod is to be downloaded, else false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Id of the mod to be updated.
    /// </summary>
    public string   ModId { get; set; }

    /// <summary>
    /// Name of the mod to be updated.
    /// </summary>
    public string ModName { get; set; }

    /// <summary>
    /// The previous version of the mod.
    /// </summary>
    public NuGetVersion  OldVersion { get; set; }

    /// <summary>
    /// The new version of the mod.
    /// </summary>
    public NuGetVersion NewVersion { get; set; }

    /// <summary>
    /// The update size for the mod.
    /// </summary>
    public long UpdateSize { get; set; }

    /// <summary>
    /// Changelog for this update, if available.
    /// </summary>
    public string? Changelog { get; set; }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Mod update size in MegaBytes.
    /// </summary>
    public float UpdateSizeMB => UpdateSize / 1000F / 1000F;

    /// <summary/>
    public ModUpdate(string modId, NuGetVersion oldVersion, NuGetVersion newVersion, long updateSize, string? changelog, string modName)
    {
        Enabled = true;
        ModId = modId;
        OldVersion = oldVersion;
        NewVersion = newVersion;
        UpdateSize = updateSize;
        Changelog = changelog;
        ModName = modName;
    }
}