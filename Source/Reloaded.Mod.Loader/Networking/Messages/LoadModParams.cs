using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Messages
{
    public class LoadModParams : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.LoadModParams;
        public string ModId { get; set; }
    }
}
