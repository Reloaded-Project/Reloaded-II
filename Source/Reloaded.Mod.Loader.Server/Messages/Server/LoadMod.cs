using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Server
{
    public struct LoadMod : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.LoadMod;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        public string ModId { get; set; }

        public LoadMod(string modId)
        {
            ModId = modId;
        }
    }
}
