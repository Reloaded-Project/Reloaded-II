namespace Reloaded.Mod.Interfaces.Internal;

public interface IApplicationConfigV2 : IApplicationConfigV1
{
    /// <summary>
    /// Data stored by plugins. Maps a unique string key to arbitrary data.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; }
}