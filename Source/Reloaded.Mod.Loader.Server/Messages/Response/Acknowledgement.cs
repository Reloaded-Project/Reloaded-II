using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Response
{
    public struct Acknowledgement : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.Acknowledgement;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        /// <summary>
        /// This member is a dummy, it has no meaning.
        /// </summary>
        public bool Null { get; set; }
    }
}
