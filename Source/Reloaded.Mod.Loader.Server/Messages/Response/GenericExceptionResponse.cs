using Reloaded.Messaging.Compression;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Serialization;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Response
{
    public struct GenericExceptionResponse : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GenericException;
        public ISerializer GetSerializer()  => new MsgPackSerializer(true);
        public ICompressor GetCompressor()  => null;

        public string Message { get; set; }

        public GenericExceptionResponse(string message) { Message = message; }
    }
}
