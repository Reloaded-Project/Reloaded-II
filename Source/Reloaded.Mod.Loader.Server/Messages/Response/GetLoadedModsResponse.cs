using Reloaded.Messaging.Compression;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Serialization;
using Reloaded.Messaging.Serializer.MessagePack;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Loader.Server.Messages.Response
{
    public struct GetLoadedModsResponse : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GetLoadedModsResponse;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        public ModInfo[] Mods { get; set; }

        public GetLoadedModsResponse(ModInfo[] mods) { Mods = mods; }
    }
}
