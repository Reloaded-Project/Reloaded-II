namespace Reloaded.Utils.Server;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public abstract class ServerBase : IDisposable
{
    protected ILogger Logger = null!;
    protected IModLoader Loader = null!;
    protected bool LogEnabled = false;

    protected ServerBase() { }

    /// <summary/>
    protected ServerBase(ILogger logger, IModLoader loader)
    {
        Logger = logger;
        Loader = loader;
    }

    /// <summary>
    /// Restarts the server with a given configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    public abstract void RestartWithConfig(Config configuration);

    /// <inheritdoc />
    public abstract void Dispose();
   
    protected void HandleSetModState(SetModState received)
    {
        if (LogEnabled)
            Logger.WriteLineAsync($"[{nameof(ServerBase)}] Setting State: {received.Type}, ModId: {received.ModId}, Key: {received.Key.Key}");
        
        switch (received.Type)
        {
            case ModStateType.Load:
                Loader!.LoadMod(received.ModId);
                break;
            case ModStateType.Unload:
                Loader!.UnloadMod(received.ModId);
                break;
            case ModStateType.Suspend:
                Loader!.SuspendMod(received.ModId);
                break;
            case ModStateType.Resume:
                Loader!.ResumeMod(received.ModId);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown Type: {received.Type}");
        }
    }

    protected ReusableSingletonMemoryStream SerializeAcknowledgement(MessageKey key)
    {
        var response = new AcknowledgementOrExceptionResponse(key);
        return response.Serialize(ref response);
    }

    protected ReusableSingletonMemoryStream HandleGetLoadedMods(MessageKey key)
    {
        if (LogEnabled)
            Logger.WriteLineAsync($"[{nameof(ServerBase)}] Retrieving Loaded Mods, Key: {key.Key}");
        
        var response = new GetLoadedModsResponse(Loader.GetLoadedMods(), key);
        return response.Serialize(ref response);
    }

    /// <summary>
    /// Applies the given configuration to the server instance.
    /// </summary>
    /// <param name="configuration">The configuration in question.</param>
    protected void SetConfiguration(Config configuration)
    {
        LogEnabled = configuration.Log;
    }
}