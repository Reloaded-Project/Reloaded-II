using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Represents an individual mod update to be performed.
/// </summary>
public class ModUpdate : ObservableObject
{
    /// <summary>
    /// Id of the mod to be updated.
    /// </summary>
    public string   ModId { get; set; }

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

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Mod update size in MegaBytes.
    /// </summary>
    public float UpdateSizeMB => UpdateSize / 1000F / 1000F;

    /// <summary/>
    public ModUpdate(string modId, NuGetVersion oldVersion, NuGetVersion newVersion, long updateSize)
    {
        ModId = modId;
        OldVersion = oldVersion;
        NewVersion = newVersion;
        UpdateSize = updateSize;
    }
}