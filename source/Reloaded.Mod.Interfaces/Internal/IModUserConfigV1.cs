namespace Reloaded.Mod.Interfaces.Internal;

public interface IModUserConfigV1
{
    /// <summary>
    /// The ID to which this mod belongs to.
    /// </summary>
    public string ModId { get; set; }

    /// <summary>
    /// Data stored by plugins. Maps a unique string key to arbitrary data.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; }

    /// <summary>
    /// True if the mod is a universal mod and should be available for every application by default.
    /// </summary>
    public bool IsUniversalMod { get; set; }

    /// <summary>
    /// True if to allow prereleases, else false.
    /// </summary>
    public bool? AllowPrereleases { get; set; }
}