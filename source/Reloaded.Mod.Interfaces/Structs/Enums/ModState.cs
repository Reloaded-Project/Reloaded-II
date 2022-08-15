namespace Reloaded.Mod.Interfaces.Structs.Enums;

/// <summary>
/// Contains the current state of a mod.
/// </summary>
public enum ModState
{
    /// <summary>
    /// The mod is currently running as usual.
    /// </summary>
    Running = 0,

    /// <summary>
    /// The mod is suspended.
    /// </summary>
    Suspended = 1
}