using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Loader.Update.Caching;

// Note: If caching is needed elsewhere, move this out to update lib. 
internal class AkavacheWebCacheStore : ICacheStore
{
    public static AkavacheWebCacheStore Instance { get; set; } = new();

    private static readonly TimeSpan MinExpiration = TimeSpan.FromDays(14);

    private MessageContentHttpMessageSerializer _messageSerializer = new(true);

    private readonly SqlRawPersistentBlobCache _cache;

    /// <summary/>
    public AkavacheWebCacheStore()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Paths.WebCachePath)!);
        _cache = new SqlRawPersistentBlobCache(Paths.WebCachePath);
        CleanupAsync();
    }

    public void Dispose() => Shutdown();

    /// <summary>
    /// Shuts down the service. Call this on application exit.
    /// </summary>
    public void Shutdown()
    {
        CleanupAsync().Wait();
        _cache.Dispose();
        _cache.Shutdown.Wait();
    }

    public IObservable<Unit> CleanupAsync()
    {
        void OnFlushCompleted(Unit unit) => _cache.Vacuum().Subscribe(OnVacuumCompleted);
        void OnVacuumCompleted(Unit unit) => _cache.Connection.ExecuteScalar<int>("PRAGMA wal_checkpoint(TRUNCATE)", Array.Empty<object>());
        var flush = _cache.Flush();
        flush.Subscribe(OnFlushCompleted);
        return flush;
    }

    public async Task<HttpResponseMessage?> GetValueAsync(CacheKey key)
    {
        var observable = _cache.Get(key.ToString());
        var buffer = await observable.Catch(Observable.Return<byte[]>(null!));
        if (buffer != null!)
        {
            await using var memoryStream = new MemoryStream(buffer);
            await using var memory = Compression.DecompressToStream(memoryStream);
            return await _messageSerializer.DeserializeToResponseAsync(memory).ConfigureAwait(false);
        }

        return null;
    }

    public async Task AddOrUpdateAsync(CacheKey key, HttpResponseMessage response)
    {
        var req = response.RequestMessage;
        response.RequestMessage = null!;
        await using var memoryStream = new MemoryStream();
        await _messageSerializer.SerializeAsync(response, memoryStream).ConfigureAwait(false);
        response.RequestMessage = req;

        // Calculate expiry
        var minExpiry = DateTimeOffset.UtcNow.Add(MinExpiration);
        var suggestedExpiry = response.GetExpiry() ?? minExpiry;
        var optimalExpiry = (suggestedExpiry > minExpiry) ? suggestedExpiry : minExpiry;
        memoryStream.Position = 0;
        var compressed = Compression.Compress(memoryStream);
        await _cache.Insert(key.ToString(), compressed, optimalExpiry);
    }

    public async Task<bool> TryRemoveAsync(CacheKey key)
    {
        try
        {
            var result = true;
            var observable = _cache.Invalidate(key.ToString());
            observable.Subscribe(unit => { }, exception =>
            {
                if (exception is KeyNotFoundException)
                    result = false;
            }, () => { });

            await observable;
            return result;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public async Task ClearAsync() => await _cache.InvalidateAll();
}