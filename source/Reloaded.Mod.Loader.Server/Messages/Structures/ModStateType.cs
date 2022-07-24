namespace Reloaded.Mod.Loader.Server.Messages.Structures;

/// <summary>
/// Requests a specific state be applied to a specified mod.
/// </summary>
public enum ModStateType
{
    /// <summary>
    /// Tells the server that the specified mod should be loaded
    /// </summary>
    Load = 0,

    /// <summary>
    /// Tells the server that the specified mod should be unloaded.
    /// </summary>
    Unload = 1,

    /// <summary>
    /// Tells the server that the specified mod should be suspended.
    /// </summary>
    Suspend = 2,

    /// <summary>
    /// Tells the server that the specified mod should be resumed.
    /// </summary>
    Resume = 3
}