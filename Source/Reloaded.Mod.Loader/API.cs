using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using LiteNetLib;
using Reloaded.Messaging;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Loader.Bootstrap;
using Reloaded.Mod.Loader.Server.Messages;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Server;

namespace Reloaded.Mod.Loader
{
    public static class API
    {
        private static SimpleHost<MessageType> _simpleHost;
        private static Loader _loader;
        private static Task _setupServerTask;

        /* Ensures DLL Resolution */
        static API()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyResolver.ResolveAssembly;

                // Setup mod loader.
                _loader = new Loader();

                // Setup host.
                // Done on another thread via Task library to avoid
                // JIT overhead affecting startup times by around ~30-70ms.
                _setupServerTask = Task.Run(SetupServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Load Reloaded-II.\n{ex.Message}");
            }
        }

        private static void SetupServer()
        {
            _simpleHost = new SimpleHost<MessageType>(true);
            AddMessageHandler<GetLoadedMods>(GetLoadedMods);
            AddMessageHandler<SuspendMod>(SuspendMod);
            AddMessageHandler<UnloadMod>(UnloadMod);
            AddMessageHandler<LoadMod>(LoadMod);
            AddMessageHandler<ResumeMod>(ResumeMod);
            _simpleHost.NetManager.Start(IPAddress.IPv6Loopback, IPAddress.Loopback, 0);
        }

        /* Custom method to add request handler with callback on exception. */
        private static void AddMessageHandler<TStruct>(MessageHandler<MessageType>.Handler<TStruct> messageHandler) where TStruct : IMessage<MessageType>, new()
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


        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Returns the port on the local machine
        /// </summary>
        public static int GetPort()
        {
            _setupServerTask.Wait();
            return _simpleHost.NetManager.LocalPort;
        }

        /* Message Handlers */
        static void GetLoadedMods(ref NetMessage<GetLoadedMods> message)
        {
            _loader.GetLoadedMods(ref message);
        }

        private static void UnloadMod(ref NetMessage<UnloadMod> netmessage)
        {
            _loader.UnloadMod(netmessage.Message.ModId);
        }

        private static void SuspendMod(ref NetMessage<SuspendMod> netmessage)
        {
            _loader.SuspendMod(netmessage.Message.ModId);
        }

        private static void LoadMod(ref NetMessage<LoadMod> netmessage)
        {
            _loader.LoadMod(netmessage.Message.ModId);
        }

        private static void ResumeMod(ref NetMessage<ResumeMod> netmessage)
        {
            _loader.ResumeMod(netmessage.Message.ModId);
        }

        /*
            ModInfo = GetLoadedMods()
            SuspendMod(string ModId)
            ResumeMod(string ModId)
            UnloadMod(string ModId)
            LoadMod(string ModId)

            // Each has a message and result structs.
            // All structs in enum (MessageType) and inhwerit from base message. 
        */
    }
}
