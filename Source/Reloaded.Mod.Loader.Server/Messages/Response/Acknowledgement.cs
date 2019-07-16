using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Messaging.Compression;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Serialization;
using Reloaded.Messaging.Serializer.MessagePack;

namespace Reloaded.Mod.Loader.Server.Messages.Response
{
    public struct Acknowledgement : IMessage<MessageType>
    {
        public MessageType GetMessageType() => MessageType.GetLoadedModsResponse;
        public ISerializer GetSerializer() => new MsgPackSerializer(true);
        public ICompressor GetCompressor() => null;

        /// <summary>
        /// This member is a dummy, it has no meaning.
        /// </summary>
        public bool Null { get; set; }
    }
}
