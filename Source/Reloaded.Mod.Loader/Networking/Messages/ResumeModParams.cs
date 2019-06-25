using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Messages
{
    public class ResumeModParams : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.ResumeModParams;
        public string ModId { get; set; }
    }
}
