using MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages
{
    [MessagePackObject]
    public class Message<TMessageType, TStruct> : MessageBase<TMessageType> where TStruct : IMessage<TMessageType>
    {
        [Key(1)]
        public TStruct ActualMessage   { get; set; }

        public Message(TStruct message)
        {
            MessageType = ((IMessage<TMessageType>) message).GetMessageType();
            ActualMessage = message;
        }

        /// <summary>
        /// Serializes the current instance and returns an array of bytes representing the instance.
        /// </summary>
        public byte[] Serialize()
        {
            return LZ4MessagePackSerializer.Serialize(this);
        }

        /// <summary>
        /// Deserializes a given set of bytes into a usable struct.
        /// </summary>
        public static Message<TMessageType, TStruct> Deserialize(byte[] serializedBytes)
        {
            return LZ4MessagePackSerializer.Deserialize<Message<TMessageType, TStruct>>(serializedBytes);
        }
    }

    /// <summary>
    /// Common interface shared by individual messages.
    /// </summary>
    public interface IMessage<TMessageType>
    {
        TMessageType GetMessageType();
    }
}
