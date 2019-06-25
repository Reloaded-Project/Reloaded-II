using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Messages
{
    public class GetLoadedModsParams : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GetLoadedModsParams;
    }
}
