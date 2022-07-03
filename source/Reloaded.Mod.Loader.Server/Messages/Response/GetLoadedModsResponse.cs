namespace Reloaded.Mod.Loader.Server.Messages.Response;

public struct GetLoadedModsResponse : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.GetLoadedModsResponse;
    public ISerializer GetSerializer() => new SystemTextJsonSerializer(MessageCommon.SerializerOptions);
    public ICompressor GetCompressor() => null;

    public ModInfo[] Mods { get; set; }

    public GetLoadedModsResponse(ModInfo[] mods) { Mods = mods; }
}