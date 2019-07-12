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
            _simpleHost.NetManager.Start(IPAddress.IPv6Loopback, IPAddress.Loopback, 0);
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
                }
            }

            _simpleHost.MessageHandler.AddOrOverrideHandler<TStruct>(Handler);
        }

        /* Message Handlers */
        void GetLoadedMods(ref NetMessage<GetLoadedMods> message)
        {
            _loader.GetLoadedMods(ref message);
        }

        void UnloadMod(ref NetMessage<UnloadMod> netmessage)
        {
            _loader.UnloadMod(netmessage.Message.ModId);
        }

        void SuspendMod(ref NetMessage<SuspendMod> netmessage)
        {
            _loader.SuspendMod(netmessage.Message.ModId);
        }

        void LoadMod(ref NetMessage<LoadMod> netmessage)
        {
            _loader.LoadMod(netmessage.Message.ModId);
        }

        void ResumeMod(ref NetMessage<ResumeMod> netmessage)
        {
            _loader.ResumeMod(netmessage.Message.ModId);
        }
    }
}
