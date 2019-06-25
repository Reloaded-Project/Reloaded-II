using MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages
{
    [MessagePackObject]
    public class MessageBase<TMessageType>
    {
        [Key(0)]
        public TMessageType MessageType { get; set; }

        /// <summary>
        /// Obtains the message type from a serialized byte array.
        /// </summary>
        public static TMessageType GetMessageType(byte[] serializedBytes)
        {
            return MessagePackSerializer.Deserialize<TMessageType>(serializedBytes);
        }
    }
}
