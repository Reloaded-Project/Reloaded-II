using System.Net;
using LiteNetLib;
using Reloaded.Mod.Loader.Server.Structs;

namespace Reloaded.Mod.Loader.Server
{
    /// <summary>
    /// Provides a simple client or host based off of LiteNetLib
    /// </summary>
    /// <typeparam name="TMessageType">The individual type </typeparam>
    public class SimpleHost<TMessageType>
    {
        public int Port => NetManager.LocalPort;
        public MessageHandler<TMessageType> MessageHandler { get; private set; }

        public EventBasedNetListener Listener { get; private set; }
        public NetManager NetManager { get; private set; }

        public SimpleHost()
        {
            MessageHandler = new MessageHandler<TMessageType>();
            Listener = new EventBasedNetListener();
            Listener.NetworkReceiveEvent += OnNetworkReceive;

            NetManager = new NetManager(Listener);
            NetManager.AutoRecycle = true;
            NetManager.UnsyncedEvents = true;
        }

        /* On each message received. */
        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliverymethod)
        {
            var rawBytes = reader.GetBytesWithLength();
            MessageHandler.Handle(new RawNetMessage(rawBytes, peer, reader, deliverymethod));
        }
    }
}
