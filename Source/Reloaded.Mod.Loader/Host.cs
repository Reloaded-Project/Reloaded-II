using System;
using System.Net;
using LiteNetLib;
using Reloaded.Messaging;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Loader.Server.Messages;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Server;

namespace Reloaded.Mod.Loader
{
    public class Host
    {
        private readonly SimpleHost<MessageType> _simpleHost;
        private readonly Loader _loader;

        public int Port => _simpleHost.NetManager.LocalPort;

        /* Setup */
        public Host(Loader loader)
        {
            _loader = loader;
            _simpleHost = new SimpleHost<MessageType>(true);
            RegisterFunctions();
            _simpleHost.NetManager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);

#if DEBUG
            _simpleHost.NetManager.DisconnectTimeout = Int32.MaxValue;
#endif
        }

        private void RegisterFunctions()
        {
            AddMessageHandler<GetLoadedMods>(GetLoadedMods);
            AddMessageHandler<SuspendMod>(SuspendMod);
            AddMessageHandler<UnloadMod>(UnloadMod);
            AddMessageHandler<LoadMod>(LoadMod);
            AddMessageHandler<ResumeMod>(ResumeMod);
        }

        /* Custom method to add request handler with callback on exception. */
        private void AddMessageHandler<TStruct>(MessageHandler<MessageType>.Handler<TStruct> messageHandler) where TStruct : IMessage<MessageType>, new()
        {
            // Function handler.
            void Handler(ref NetMessage<TStruct> netMessage)
            {
                try
                {
                    messageHandler(ref netMessage);
                }
                catch (Exception ex)
                {
                    var message = new Message<MessageType, GenericExceptionResponse>(new GenericExceptionResponse(ex.Message));
                    netMessage.Peer.Send(message.Serialize(), DeliveryMethod.ReliableOrdered);
                    netMessage.Peer.Flush();
                }
            }

            _simpleHost.MessageHandler.AddOrOverrideHandler<TStruct>(Handler);
        }

        /* Message Handlers */
        void GetLoadedMods(ref NetMessage<GetLoadedMods> message)
        {
            var summary = _loader.GetLoadedModSummary();
            var messageToSend = new Message<MessageType, GetLoadedModsResponse>(new GetLoadedModsResponse(summary.ToArray()));
            message.Peer.Send(messageToSend.Serialize(), DeliveryMethod.ReliableOrdered);
        }

        void UnloadMod(ref NetMessage<UnloadMod> netMessage)
        {
            _loader.UnloadMod(netMessage.Message.ModId);
            SendAck(netMessage.Peer);
        }

        void SuspendMod(ref NetMessage<SuspendMod> netMessage)
        {
            _loader.SuspendMod(netMessage.Message.ModId);
            SendAck(netMessage.Peer);
        }

        void LoadMod(ref NetMessage<LoadMod> netMessage)
        {
            _loader.LoadMod(netMessage.Message.ModId);
            SendAck(netMessage.Peer);
        }

        void ResumeMod(ref NetMessage<ResumeMod> netMessage)
        {
            _loader.ResumeMod(netMessage.Message.ModId);
            SendAck(netMessage.Peer);
        }

        /* Helpers */
        private void SendAck(NetPeer peer)
        {
            var message = new Message<MessageType, Acknowledgement>(new Acknowledgement());
            peer.Send(message.Serialize(), DeliveryMethod.ReliableOrdered);
        }
    }
}
