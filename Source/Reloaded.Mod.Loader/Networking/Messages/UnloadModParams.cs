using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Messages
{
    public class UnloadModParams : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.UnloadModParams;
        public string ModId { get; set; }
    }
}
