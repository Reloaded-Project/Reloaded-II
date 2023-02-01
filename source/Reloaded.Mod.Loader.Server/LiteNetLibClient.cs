namespace Reloaded.Mod.Loader.Server;

/// <summary>
/// Encapsulates an individual client used to communicate with a host over the network.
/// </summary>
public class LiteNetLibClient : ClientBase, ILiteNetLibRefAction<AcknowledgementOrExceptionResponse>, ILiteNetLibRefAction<GetLoadedModsResponse>
{
    public event Action<AcknowledgementOrExceptionResponse>? OnReceiveException;

    /// <summary>
    /// Executed when the connection is gained.
    /// </summary>
    public event Action<NetPeer>? OnConnected;

    /// <summary>
    /// Executed when the connection is lost and reconnection (if enabled) has failed.
    /// </summary>
    public event Action<NetPeer>? OnConnectionLost;

    /// <summary>
    /// Executed when the connection is lost and a reconnection attempt is about to be made.
    /// </summary>
    public event Action<NetPeer>? OnTryReconnect;

    /// <summary>
    /// A function that retrieves new server details on reconnect attempt.
    /// </summary>
    public event Func<(string? password, int? port)>? OverrideDetailsOnReconnect;

    /// <summary>
    /// Provides access to the underlying host used for sending and receiving messages.
    /// </summary>
    public LiteNetLibHost<MessageDispatcher<LiteNetLibState>> Host;

    /// <summary>
    /// A dictionary containing callbacks for corresponding request IDs.
    /// </summary>
    public ConcurrentDictionary<MessageKey, Action<object>> GetValueCallback = new ConcurrentDictionary<MessageKey, Action<object>>();

    private bool _tryReconnectOnShutdown;
    private string _password;
    private int _port;
    private IPAddress _address;

    /// <summary>
    /// Encapsulates an individual client used to send messages over the network.
    /// </summary>
    /// <param name="address">The host's address, either ipv4 or ipv6.</param>
    /// <param name="password">The password used to connect to the host.</param>
    /// <param name="port">The port of the host to connect to.</param>
    /// <param name="tryReconnectOnConnectionLoss">Tries to automatically reconnect on loss of connection.</param>
    public LiteNetLibClient(IPAddress address, string password, int port, bool tryReconnectOnConnectionLoss = true)
    {
        _address = address;
        _password = password;
        _port = port;
        _tryReconnectOnShutdown = tryReconnectOnConnectionLoss;

        Host = new LiteNetLibHost<MessageDispatcher<LiteNetLibState>>(false, new MessageDispatcher<LiteNetLibState>(), _password);
        new AcknowledgementOrExceptionResponse().AddToDispatcher(this, ref Host.Dispatcher);
        new GetLoadedModsResponse().AddToDispatcher(this, ref Host.Dispatcher);

        Host.Listener.PeerDisconnectedEvent += OnPeerDisconnected;
        Host.Listener.PeerConnectedEvent += OnPeerConnected;
        Host.Manager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);
        Connect(_password, port);

#if DEBUG
        Host.Manager.DisconnectTimeout = Int32.MaxValue;
#endif
    }

    public bool IsConnected => Host.FirstPeer != null && Host.FirstPeer.ConnectionState == ConnectionState.Connected;

    private NetPeer? Connect(string password, int port) => Host.Manager.Connect(new IPEndPoint(_address, port), password);

    private void OnPeerConnected(NetPeer peer) => OnConnected?.Invoke(peer);

    private async void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (!_tryReconnectOnShutdown)
        {
            OnConnectionLost?.Invoke(peer);
            return;
        }

        const int reconnectMaxTimeMs = 30000;
        const int reconnectSleepTimeMs = 500;
        OnTryReconnect?.Invoke(peer);
        var watch = Stopwatch.StartNew();
        NetPeer? newPeer = null;

        while (true)
        {
            var password = _password;
            var port     = _port;

            // Try get new details if necessary.
            var details = OverrideDetailsOnReconnect?.Invoke();
            if (details.HasValue)
            {
                var detailsValue = details.Value;
                password = detailsValue.password ?? password;
                port = detailsValue.port ?? port;
            }

            // Try reconnect.
            if (port != -1)
                break;
                
            newPeer = Connect(password, port);
            if (newPeer != null)
                break;
                
            if (watch.ElapsedMilliseconds > reconnectMaxTimeMs)
                break;

            await Task.Delay(reconnectSleepTimeMs).ConfigureAwait(false);
        }

        if (newPeer == null)
            OnConnectionLost?.Invoke(peer);
    }

    #region Receive: Exception
    public void OnMessageReceive(ref AcknowledgementOrExceptionResponse received, ref LiteNetLibState data)
    {
        if (GetValueCallback.TryGetValue(received.Key, out var action))
            action(received);
    }
    #endregion

    #region Receive: Loaded Mods
    public void OnMessageReceive(ref GetLoadedModsResponse received, ref LiteNetLibState data)
    {
        if (GetValueCallback.TryGetValue(received.Key, out var action))
            action(received);
    }
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override async Task<AnyOf<AcknowledgementOrExceptionResponse, TResponse>> SendRequest<TStruct, TResponse>(TStruct structure, int timeout, CancellationToken token)
    {   
        // Serialize structure
        var key = GetNextKey();
        structure.Key = key;
        AnyOf<AcknowledgementOrExceptionResponse, TResponse>? response = null;
        var semaphore = new SemaphoreSlim(0);

        GetValueCallback[key] = (item) =>
        {
            if (item is AcknowledgementOrExceptionResponse exceptionResponse)
            {
                response = new AnyOf<AcknowledgementOrExceptionResponse, TResponse>(exceptionResponse);
                semaphore.Release();
            }
            else if (item.GetType() == typeof(TResponse))
            {
                response = new AnyOf<AcknowledgementOrExceptionResponse, TResponse>((TResponse)item);
                semaphore.Release();
            }
        };

        try
        {
            using (var packed = structure.Pack())
            {
                Host.SendFirstPeer(packed.Span);
            }

            await semaphore.WaitAsync(timeout, token);
            var value = response.GetValueOrDefault(new AcknowledgementOrExceptionResponse($"No response from server. Timeout Exceeded.", null));
            if (value.IsFirst && value.First.IsException())
                OnReceiveException?.Invoke(value.First);

            return value;
        }
        finally
        {
            GetValueCallback.Remove(key, out _);
        }
    }
}