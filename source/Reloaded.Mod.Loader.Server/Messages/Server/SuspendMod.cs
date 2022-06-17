using System.Text.Json;
using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.SystemTextJson;

namespace Reloaded.Mod.Loader.Server.Messages.Server;

public struct SuspendMod : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.SuspendMod;
    public ISerializer GetSerializer() => new SystemTextJsonSerializer(MessageCommon.SerializerOptions);
    public ICompressor GetCompressor() => null;

    public string ModId { get; set; }

    public SuspendMod(string modId)
    {
        ModId = modId;
    }
}