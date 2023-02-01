namespace Reloaded.Mod.Loader.Server.Messages.Structures;

/// <summary>
/// Contains the information about a loaded-in mod.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class ServerModInfo : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current ID of the mod.
    /// </summary>
    [IgnoreDuringEquals]
    public string ModId => Config.ModId;

    /// <summary>
    /// The current state of the mod.
    /// </summary>
    public ModState State { get; set; }

    /// <summary>
    /// The individual full configuration of this mod.
    /// </summary>
    public ModConfig Config { get; set; }

    /// <summary>
    /// True if this mod can be suspended.
    /// </summary>
    public bool CanSuspend { get; set; }

    /// <summary>
    /// True if this mod can be unloaded.
    /// </summary>
    public bool CanUnload { get; set; }

    /// <summary>
    /// Contains information about a loaded in mod.
    /// </summary>
    /// <param name="state">The current state of the mod.</param>
    /// <param name="modConfig">The current mod configuration.</param>
    /// <param name="canSuspend">Whether the mod can be suspended.</param>
    /// <param name="canUnload">Whether the mod can be reloaded.</param>
    public ServerModInfo(ModState state, ModConfig modConfig, bool canSuspend, bool canUnload)
    {
        State = state;
        Config = modConfig;
        CanSuspend = canSuspend;
        CanUnload = canUnload;
    }

    /// <summary>
    /// For serializers.
    /// </summary>
    public ServerModInfo() { }

    /// <summary>
    /// Returns true if the client can send a suspend command for this mod.
    /// </summary>
    public bool CanSendSuspend => State == ModState.Running && CanSuspend;

    /// <summary>
    /// Returns true if the client can send a resume command for this mod.
    /// </summary>
    public bool CanSendResume => State == ModState.Suspended && CanSuspend;

    // Only because we data bind to this, don't wanna cause mem leak.

    public event PropertyChangedEventHandler? PropertyChanged;
}