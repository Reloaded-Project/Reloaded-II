namespace Reloaded.Utils.Server;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class LiteNetLibServer : ServerBase, ILiteNetLibRefAction<SetModState>, ILiteNetLibRefAction<GetLoadedMods>
{
    public LiteNetLibHost<MessageDispatcher<LiteNetLibState>>? Host;
    
    private LiteNetLibServer() { }

    private LiteNetLibServer(ILogger logger, IModLoader loader) : base(logger, loader) { }

    /// <summary>
    /// Asynchronously starts the host.
    /// </summary>
    public static LiteNetLibServer Create(ILogger logger, IModLoader modLoader, Config config)
    {
        var mod  = new LiteNetLibServer(logger, modLoader);
        mod.Host = new LiteNetLibHost<MessageDispatcher<LiteNetLibState>>(true, new MessageDispatcher<LiteNetLibState>());
        mod.Host.ConnectionRequestEvent += mod.HandleConnectionRequest;
        
        new SetModState().AddToDispatcher(mod, ref mod.Host.Dispatcher);
        new GetLoadedMods().AddToDispatcher(mod, ref mod.Host.Dispatcher);

        mod.RestartWithConfig(config);
        return mod;
    }

    private void HandleConnectionRequest(ConnectionRequest request)
    {
        // Always accept local connections.
        var addr = request.RemoteEndPoint.Address;
        if (addr.Equals(IPAddress.IPv6Loopback) || addr.Equals(IPAddress.Loopback))
        {
            request.Accept();
            return;
        }

        if (Host!.AcceptClients)
            request.AcceptIfKey(Host.Password);
        else
            request.Reject();
    }

    /// <summary>
    /// Restarts the server with a given configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    public override void RestartWithConfig(Config configuration)
    {
        if (Host!.Manager.IsRunning)
            Host.Manager.Stop();

        SetConfiguration(configuration);
        var config = configuration.LiteNetLibConfig;
        if (!config.Enable)
            return;

        Host.Password = config.Password;
        if (config.AllowExternalConnections)
            Host.Manager.Start(IPAddress.Any, IPAddress.IPv6Any, config.Port);
        else
            Host.Manager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, config.Port);

        ServerUtility.WriteServerPort(Process.GetCurrentProcess().Id, Host.Manager.LocalPort);
    }

    /// <inheritdoc />
    public override void Dispose() => Host?.Dispose();

    // SetModState
    public void OnMessageReceive(ref SetModState received, ref LiteNetLibState data)
    {
        try 
        { 
            HandleSetModState(received);
            using var serialized = SerializeAcknowledgement(received.Key);
            data.Peer.Send(serialized.Span, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex) { HandleException(ref data, received.Key, ex); }
    }

    // GetLoadedMods
    public void OnMessageReceive(ref GetLoadedMods received, ref LiteNetLibState data)
    {
        try
        {
            using var serialized = HandleGetLoadedMods(received.Key);
            data.Peer.Send(serialized.Span, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex) { HandleException(ref data, received.Key, ex); }
    }

    /// <summary>
    /// Handles a generic exception.
    /// </summary>
    private void HandleException(ref LiteNetLibState data, MessageKey key, Exception ex)
    {
        var message = new AcknowledgementOrExceptionResponse(ex.Message, ex.StackTrace, key);
        Logger.WriteLineAsync($"[{nameof(LiteNetLibServer)}] Sending Exception: {ex.Message}\nTrace: {ex.StackTrace}");
        using var serialized = message.Serialize(ref message);
        data.Peer.Send(serialized.Span, DeliveryMethod.ReliableOrdered);
    }
}