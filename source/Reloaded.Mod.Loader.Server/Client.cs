namespace Reloaded.Mod.Loader.Server
{
    public class Client
    {
        public event Action<GenericExceptionResponse> OnReceiveException;

        private NetPeer Server => _simpleHost.NetManager.FirstPeer;
        private readonly SimpleHost<MessageType> _simpleHost;
        private const int DefaultTimeout = 3000;

        public Client(int port)
        {
            _simpleHost = new SimpleHost<MessageType>(false);
            _simpleHost.NetManager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);
            _simpleHost.NetManager.Connect(new IPEndPoint(IPAddress.Loopback, port), "");

#if DEBUG
            _simpleHost.NetManager.DisconnectTimeout = Int32.MaxValue;
#endif

            _simpleHost.MessageHandler.AddOrOverrideHandler<GenericExceptionResponse>(ReceiveException);
        }

        /* Relay exceptions to event. */
        private void ReceiveException(ref NetMessage<GenericExceptionResponse> message)
        {
            OnReceiveException?.Invoke(message.Message);
        }

        /* Utility functions. */

        /// <summary>
        /// Returns true if the mod loader is loaded and present inside a specified process. Else returns false.
        /// </summary>
        /// <param name="process">The process to check for.</param>
        public static bool IsModLoaderPresent(Process process)
        {
            try
            {
                Client.GetPort(process.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to acquire the port to connect to a remote process with a specific id.
        /// </summary>
        /// <exception cref="FileNotFoundException"><see cref="MemoryMappedFile"/> was not created by the mod loader, in other words, mod loader is not loaded.</exception>
        /// <returns>0 if Reloaded is still initializing, exception if not initialized, else a valid port.</returns>
        public static int GetPort(int pid)
        {
            var mappedFile = MemoryMappedFile.OpenExisting(ServerUtility.GetMappedFileNameForPid(pid));
            var view = mappedFile.CreateViewStream();
            var binaryReader = new BinaryReader(view);
            return binaryReader.ReadInt32();
        }

        /* Server calls without responses. */
        public Task<GetLoadedModsResponse> GetLoadedModsAsync(int timeout = DefaultTimeout, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<GetLoadedMods, GetLoadedModsResponse>(new GetLoadedMods(), timeout, token);
        }

        public Task<Acknowledgement> LoadMod(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<LoadMod, Acknowledgement>(new LoadMod(modId), timeout, token);
        }

        public Task<Acknowledgement> ResumeModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<ResumeMod, Acknowledgement>(new ResumeMod(modId), timeout, token);
        }

        public Task<Acknowledgement> SuspendModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<SuspendMod, Acknowledgement>(new SuspendMod(modId), timeout, token);
        }

        public Task<Acknowledgement> UnloadModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
        {
            return SendMessageWithResponseAsync<UnloadMod, Acknowledgement>(new UnloadMod(modId), timeout, token);
        }

        /* Send synchronously with timeout */
        private async Task<TResponse> SendMessageWithResponseAsync<TStruct, TResponse>(TStruct message, int timeout = DefaultTimeout, CancellationToken token = default) 
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