namespace Reloaded.Mod.Loader.Server.Messages.Server;

public struct ResumeMod : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.ResumeMod;
    public ISerializer GetSerializer() => new SystemTextJsonSerializer(MessageCommon.SerializerOptions);
    public ICompressor GetCompressor() => null;

    public string ModId { get; set; }

    public ResumeMod(string modId)
    {
        ModId = modId;
    }
}