using SetModState = Reloaded.Mod.Loader.Server.Messages.Requests.SetModState;

namespace Reloaded.Mod.Loader.Server;

/// <summary>
/// Encapsulates an individual client used to communicate with a host over the network.
/// </summary>
public abstract class ClientBase
{
    private const int DefaultTimeout = 5000;
    private uint _currentKey;

    /// <summary>
    /// Encapsulates an individual client used to send messages over the network.
    /// </summary>
    public ClientBase() { }

    /// <summary>
    /// Increments the current message key by 1 and returns the result.
    /// </summary>
    public MessageKey GetNextKey() => (MessageKey)Interlocked.Increment(ref _currentKey);

    /// <summary>
    /// Retrieves the list of currently loaded in mods.
    /// </summary>
    /// <param name="timeout">Timeout for the operation.</param>
    /// <param name="token">Token used for cancelling the operation.</param>
    /// <returns>Task signalling completion of the request.</returns>
    public Task<AnyOf<AcknowledgementOrExceptionResponse, GetLoadedModsResponse>> GetLoadedModsAsync(int timeout = DefaultTimeout, CancellationToken token = default)
    {
        return SendRequest<GetLoadedMods, GetLoadedModsResponse>(new GetLoadedMods(), timeout, token);
    }

    /// <summary>
    /// Loads mod with a given ID.
    /// </summary>
    /// <param name="modId">ID of the mod to load.</param>
    /// <param name="timeout">Timeout to perform the action.</param>
    /// <param name="token">Token for cancelling the task.</param>
    /// <returns>Task signalling completion of request.</returns>
    public Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>> LoadModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
    {
        return SendRequest<SetModState, NullResponse>(new SetModState(modId, ModStateType.Load), timeout, token);
    }

    /// <summary>
    /// Resumes mod with a given ID.
    /// </summary>
    /// <param name="modId">ID of the mod to resume.</param>
    /// <param name="timeout">Timeout to perform the action.</param>
    /// <param name="token">Token for cancelling the task.</param>
    /// <returns>Task signalling completion of request.</returns>
    public Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>> ResumeModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
    {
        return SendRequest<SetModState, NullResponse>(new SetModState(modId, ModStateType.Resume), timeout, token);
    }

    /// <summary>
    /// Suspends mod with a given ID.
    /// </summary>
    /// <param name="modId">ID of the mod to resume.</param>
    /// <param name="timeout">Timeout to perform the action.</param>
    /// <param name="token">Token for cancelling the task.</param>
    /// <returns>Task signalling completion of request.</returns>
    public Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>> SuspendModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
    {
        return SendRequest<SetModState, NullResponse>(new SetModState(modId, ModStateType.Suspend), timeout, token);
    }

    /// <summary>
    /// Unloads mod with a given ID.
    /// </summary>
    /// <param name="modId">ID of the mod to resume.</param>
    /// <param name="timeout">Timeout to perform the action.</param>
    /// <param name="token">Token for cancelling the task.</param>
    /// <returns>Task signalling completion of request.</returns>
    public Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>> UnloadModAsync(string modId, int timeout = DefaultTimeout, CancellationToken token = default)
    {
        return SendRequest<SetModState, NullResponse>(new SetModState(modId, ModStateType.Unload), timeout, token);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract Task<AnyOf<AcknowledgementOrExceptionResponse, TResponse>> SendRequest<TStruct, TResponse>(TStruct structure, int timeout, CancellationToken token)
        where TStruct : IKeyedMessage, IPackable;
}