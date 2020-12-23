using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Server
{
    public struct ResumeMod : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.ResumeMod;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        public string ModId { get; set; }

        public ResumeMod(string modId)
        {
            ModId = modId;
        }
    }
}
