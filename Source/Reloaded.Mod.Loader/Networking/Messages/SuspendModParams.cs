using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Messages
{
    public class SuspendModParams : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.SuspendModParams;
        public string ModId { get; set; }
    }
}
