namespace Reloaded.Mod.Loader.Server.Messages;

/// <summary>
/// Contains all message types handled at the server level.
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// [Request] Requests a specific state to be applied to a mod.
    /// Load/Unload/Suspend/Reload. etc.
    /// </summary>
    SetModState = 0,

    /// <summary>
    /// [Request + Response]<br/>
    /// Client: Asks for list of currently loaded mods.<br/>
    /// Server: Responds with list of currently loaded mods.
    /// </summary>
    GetLoadedMods = 1,

    /// <summary>
    /// [Response] Returns an exception from the server.
    /// </summary>
    Acknowledgement = 2
}