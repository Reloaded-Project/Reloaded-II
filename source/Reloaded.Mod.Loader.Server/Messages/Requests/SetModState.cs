namespace Reloaded.Mod.Loader.Server.Messages.Requests;

/// <summary>
/// Sends a request to the host to change the state of a given mod; such as loading or unloading it.
/// </summary>
public struct SetModState : IMessage<SetModState, MessagePackSerializer<SetModState>, NullCompressor>, IKeyedMessage, IPackable
{
    public sbyte GetMessageType() => (sbyte)MessageType.SetModState;
    public MessagePackSerializer<SetModState> GetSerializer() => new();
    public NullCompressor? GetCompressor() => null!;

    /// <summary>
    /// The mod whose state is to be set.
    /// </summary>
    public string ModId { get; set; }

    /// <summary>
    /// The state to apply to the mod.
    /// </summary>
    public ModStateType Type { get; set; }

    /// <inheritdoc/>
    public MessageKey Key { get; set; }

    public SetModState(string modId, ModStateType type, MessageKey key = default)
    {
        ModId = modId;
        Type = type;
        Key = key;
    }

    /// <inheritdoc/>
    public ReusableSingletonMemoryStream Pack() => this.Serialize(ref this);
}