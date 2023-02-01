namespace Reloaded.Mod.Loader.Server.Messages.Responses;

/// <summary>
/// Response that returns the list of currently loaded in mods.
/// </summary>
public struct GetLoadedModsResponse : IMessage<GetLoadedModsResponse, MessagePackSerializer<GetLoadedModsResponse>, BrotliCompressor>, IKeyedMessage
{
    public sbyte GetMessageType() => (sbyte)MessageType.GetLoadedMods;
    public MessagePackSerializer<GetLoadedModsResponse> GetSerializer() => new();
    public BrotliCompressor GetCompressor() => new();

    /// <summary>
    /// List of currently loaded mod instances.
    /// </summary>
    public ServerModInfo[] Mods { get; set; }

    /// <inheritdoc/>
    public MessageKey Key { get; set; }

    /// <summary>
    /// Returns a list of loaded mods.
    /// </summary>
    /// <param name="mods">The list of loaded-in mods.</param>
    /// <param name="key">Individual key associated with the original message.</param>
    public GetLoadedModsResponse(ModInfo[] mods, MessageKey key = default)
    {
        Mods = GC.AllocateUninitializedArray<ServerModInfo>(mods.Length);
        for (int x = 0; x < Mods.Length; x++)
            Mods[x] = mods[x].Adapt<ServerModInfo>();

        Key = key;
    }

    /// <summary>
    /// For serialization.
    /// </summary>
    public GetLoadedModsResponse() 
    {
        Mods = Array.Empty<ServerModInfo>();
        Key = new MessageKey();
    }
}