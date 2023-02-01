namespace Reloaded.Mod.Interfaces.Internal;

public interface IModConfigV4 : IModConfigV3
{
    /// <summary>
    /// True if the mod is a library,
    /// i.e. it is intended to be consumed by other mods and cannot be explicitly enabled by the user.
    /// </summary>
    bool IsLibrary { get; set; }
}