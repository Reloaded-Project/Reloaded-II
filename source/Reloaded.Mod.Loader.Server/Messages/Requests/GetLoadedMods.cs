namespace Reloaded.Mod.Loader.Server.Messages.Requests;

/// <summary>
/// Sends a request to the host to retrieve the currently loaded in mods.
/// </summary>
public struct GetLoadedMods : IMessage<GetLoadedMods, MessagePackSerializer<GetLoadedMods>, NullCompressor>, IKeyedMessage, IPackable
{
    public sbyte GetMessageType() => (sbyte)MessageType.GetLoadedMods;
    public MessagePackSerializer<GetLoadedMods> GetSerializer() => new();
    public NullCompressor? GetCompressor() => null;

    /// <inheritdoc/>
    public MessageKey Key { get; set; }

    public GetLoadedMods(MessageKey key = default) { Key = key; }

    /// <inheritdoc/>
    public ReusableSingletonMemoryStream Pack() => this.Serialize(ref this);
}