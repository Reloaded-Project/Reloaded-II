using MessagePack;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server.Messages;

namespace Reloaded.Mod.Loader.Networking.Returns
{
    [MessagePackObject(true)]
    public class GetLoadedModsResponse : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GetLoadedModsResponse;
        public ModInfo[] Mods { get; set; }
    }
}
