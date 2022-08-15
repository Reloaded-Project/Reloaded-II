namespace Reloaded.Mod.Interfaces.Internal;

public interface IModConfigV5 : IModConfigV4
{
    /// <summary>
    /// The file name associated with the release metadata for the mod.
    /// </summary>
    public string ReleaseMetadataFileName { get; set; }

    /// <summary>
    /// Data stored by plugins. Maps a unique string key to arbitrary data.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; }

    /// <summary>
    /// True if the mod is a universal mod and should be available for every application by default.
    /// </summary>
    public bool IsUniversalMod { get; set; } 
}