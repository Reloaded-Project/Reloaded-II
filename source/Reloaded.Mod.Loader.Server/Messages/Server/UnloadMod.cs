using System.Text.Json;
using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.SystemTextJson;

namespace Reloaded.Mod.Loader.Server.Messages.Server;

public struct UnloadMod : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.UnloadMod;
    public ISerializer GetSerializer() => new SystemTextJsonSerializer(JsonSerializerOptions.Default);
    public ICompressor GetCompressor() => null;

    public string ModId { get; set; }

    public UnloadMod(string modId)
    {
        ModId = modId;
    }
}