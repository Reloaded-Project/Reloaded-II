using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Server
{
    public struct SuspendMod : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.SuspendMod;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        public string ModId { get; set; }

        public SuspendMod(string modId)
        {
            ModId = modId;
        }
    }
}
