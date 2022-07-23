using AnyOfTypes;
using Reloaded.Messaging;
using Reloaded.Messaging.Host.LiteNetLib;
using Reloaded.Messaging.Utilities;
using Reloaded.Mod.Loader.Server.Messages.Interfaces;
using Reloaded.Mod.Loader.Server.Messages.Responses;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Reloaded.Mod.Loader.Server;

/// <summary>
/// Encapsulates an individual client used to communicate with a host over the network.
/// </summary>
public class LiteNetLibClient : ClientBase, ILiteNetLibRefAction<AcknowledgementOrExceptionResponse>, ILiteNetLibRefAction<GetLoadedModsResponse>
{
    public event Action<AcknowledgementOrExceptionResponse>? OnReceiveException;

    /// <summary>
    /// Provides access to the underlying host used for sending and receiving messages.
    /// </summary>
    public LiteNetLibHost<MessageDispatcher<LiteNetLibState>> Host;

    /// <summary>
    /// A dictionary containing callbacks for corresponding request IDs.
    /// </summary>
    public ConcurrentDictionary<MessageKey, Action<object>> GetValueCallback = new ConcurrentDictionary<MessageKey, Action<object>>();

    /// <summary>
    /// Encapsulates an individual client used to send messages over the network.
    /// </summary>
    /// <param name="address">The host's address, either ipv4 or ipv6.</param>
    /// <param name="password">The password used to connect to the host.</param>
    /// <param name="port">The port of the host to connect to.</param>
    public LiteNetLibClient(IPAddress address, string password, int port)
    {
        Host = new LiteNetLibHost<MessageDispatcher<LiteNetLibState>>(false, new MessageDispatcher<LiteNetLibState>(), password);
        new AcknowledgementOrExceptionResponse().AddToDispatcher(this, ref Host.Dispatcher);
        new GetLoadedModsResponse().AddToDispatcher(this, ref Host.Dispatcher);

        Host.Manager.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);
        Host.Manager.Connect(new IPEndPoint(address, port), password);

#if DEBUG
        Host.Manager.DisconnectTimeout = Int32.MaxValue;
#endif
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
            if (item.GetType() == typeof(AcknowledgementOrExceptionResponse))
            {
                response = new AnyOf<AcknowledgementOrExceptionResponse, TResponse>((AcknowledgementOrExceptionResponse)item);
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