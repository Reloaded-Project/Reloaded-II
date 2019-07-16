using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using Reloaded.Messaging;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Loader.Server.Messages;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Server;

namespace Reloaded.Mod.Loader.Server
{
    public class Client
    {
        public event Action<GenericExceptionResponse> OnReceiveException;

        private NetPeer Server => _simpleHost.NetManager.FirstPeer;
        private readonly SimpleHost<MessageType> _simpleHost;

        public Client(int port)
        {
            _simpleHost = new SimpleHost<MessageType>(false);
            _simpleHost.NetManager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);
            _simpleHost.NetManager.Connect(new IPEndPoint(IPAddress.Loopback, port), "");
            _simpleHost.MessageHandler.AddOrOverrideHandler<GenericExceptionResponse>(ReceiveException);
        }

        /* Relay exceptions to event. */
        private void ReceiveException(ref NetMessage<GenericExceptionResponse> message)
        {
            OnReceiveException?.Invoke(message.Message);
        }

        /* Server calls without responses. */
        public Task<GetLoadedModsResponse> GetLoadedModsAsync(int timeout = 1000, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<GetLoadedMods, GetLoadedModsResponse>(new GetLoadedMods(), timeout, token);
        }

        public Task<Acknowledgement> LoadMod(string modId, int timeout = 1000, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<LoadMod, Acknowledgement>(new LoadMod(modId), timeout, token);
        }

        public Task<Acknowledgement> ResumeMod(string modId, int timeout = 1000, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<ResumeMod, Acknowledgement>(new ResumeMod(modId), timeout, token);
        }

        public Task<Acknowledgement> SuspendMod(string modId, int timeout = 1000, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<SuspendMod, Acknowledgement>(new SuspendMod(modId), timeout, token);
        }

        public Task<Acknowledgement> UnloadMod(string modId, int timeout = 1000, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<UnloadMod, Acknowledgement>(new UnloadMod(modId), timeout, token);
        }

        /* Send synchronously with timeout */
        private async Task<TResponse> SendMessageWithResponseAsync<TStruct, TResponse>(TStruct message, int timeout = 1000, CancellationToken token = default) 
            where TStruct : IMessage<MessageType>, new() 
            where TResponse : struct, IMessage<MessageType>
        {
            /* Start timeout. */
            Stopwatch watch = new Stopwatch();
            watch.Start();

            /* Setup response. */
            TResponse? response = null;
            void ReceiveMessage(ref NetMessage<TResponse> netMessage)
            {
                response = netMessage.Message;
            }

            _simpleHost.MessageHandler.AddOrOverrideHandler<TResponse>(ReceiveMessage);

            /* Send message. */
            var data = new Message<MessageType, TStruct>(message).Serialize();
            Server.Send(data, DeliveryMethod.ReliableOrdered);

            /* Wait loop. */
            while (watch.ElapsedMilliseconds < timeout)
            {
                if (token.IsCancellationRequested)
                    throw new Exception("Task was cancelled.");

                // Return response if available.
                if (response != null)
                    return response.Value;

                await Task.Delay(1, token);
            }

            throw new Exception("Timeout to receive response has expired.");
        }
    }
}
