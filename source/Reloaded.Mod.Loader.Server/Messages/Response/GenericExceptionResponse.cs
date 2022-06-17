using System.Text.Json;
using Reloaded.Messaging.Interfaces;
using Reloaded.Messaging.Serializer.SystemTextJson;

namespace Reloaded.Mod.Loader.Server.Messages.Response;

public struct GenericExceptionResponse : IMessage<MessageType>
{
    public MessageType GetMessageType() => MessageType.GenericException;
    public ISerializer GetSerializer()  => new SystemTextJsonSerializer(MessageCommon.SerializerOptions);
    public ICompressor GetCompressor()  => null;

    public string Message { get; set; }

    public GenericExceptionResponse(string message) { Message = message; }
}