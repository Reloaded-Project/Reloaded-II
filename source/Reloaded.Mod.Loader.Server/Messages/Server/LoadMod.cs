namespace Reloaded.Mod.Loader.Server.Messages.Server;

public struct LoadMod : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.LoadMod;
    public ISerializer GetSerializer() => new SystemTextJsonSerializer(MessageCommon.SerializerOptions);
    public ICompressor GetCompressor() => null;

    public string ModId { get; set; }

    public LoadMod(string modId)
    {
        ModId = modId;
    }
}