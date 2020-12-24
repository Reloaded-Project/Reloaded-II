using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Server
{
    public struct GetLoadedMods : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GetLoadedMods;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        /// <summary>
        /// This member is a dummy, it has no meaning.
        /// </summary>
        public bool Null { get; set; }
    }
}
