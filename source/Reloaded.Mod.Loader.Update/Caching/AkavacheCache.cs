namespace Reloaded.Mod.Loader.Update.Caching;

/// <summary>
/// A generic reusable caching implementation built over Akavache.
/// </summary>
public class AkavacheCache
{
    private readonly SqlRawPersistentBlobCache _cache;

    /// <summary/>
    public AkavacheCache(string absoluteFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(absoluteFilePath)!);
        _cache = new SqlRawPersistentBlobCache(absoluteFilePath);
    }

    /// <summary>
    /// Shuts down the service. Call this on application exit.
    /// </summary>
    public void Shutdown()
    {
        _cache.Flush().Wait();
        _cache.Vacuum().Wait();
        _cache.Connection.ExecuteScalar<int>("PRAGMA wal_checkpoint(TRUNCATE)", Array.Empty<object>());
        _cache.Dispose();
        _cache.Shutdown.Wait();
    }

    /// <summary>
    /// Retrieves a file for a given URI.
    /// </summary>
    /// <param name="uri">The URI to get image for.</param>
    /// <param name="expiration">How long should the file persist in the cache.</param>
    /// <param name="refreshKey">If enabled, refreshes the key on successful cache hit, extending the lifetime by expiration.</param>
    /// <param name="token">Token that can be used to cancel the operation.</param>
    public async ValueTask<byte[]> GetOrDownloadFileFromUrl(Uri uri, TimeSpan expiration, bool refreshKey = false, CancellationToken token = default) => await GetOrDownloadFileFromUrl(uri, uri.ToString(), expiration, refreshKey, token);

    /// <summary>
    /// Retrieves a file for a given URI, with custom key.
    /// </summary>
    /// <param name="uri">The URI to get image for.</param>
    /// <param name="key">The key to use for the URI.</param>
    /// <param name="refreshKey">If enabled, refreshes the key on successful cache hit, extending the lifetime by expiration.</param>
    /// <param name="expiration">How long should the file persist in the cache.</param>
    /// <param name="token">Token that can be used to cancel the operation.</param>
    public async ValueTask<byte[]> GetOrDownloadFileFromUrl(Uri uri, string key, TimeSpan expiration, bool refreshKey, CancellationToken token = default)
    {
        // Check the cache.
        var file = await GetExistingFile(key);
        if (file != null)
        {
            if (refreshKey)
                RefreshKey(key, expiration);

            return file;
        }
        
        // Get it, and put into cache.
        var data = await SharedHttpClient.UncachedAndCompressed.GetByteArrayAsync(uri, token);
        if (!token.IsCancellationRequested)
            _ = _cache.Insert(key, data, expiration);
        
        return data;
    }

    /// <summary>
    /// Retrieves a file for a given URI.
    /// Uses Brotli compression.
    /// </summary>
    /// <param name="uri">The URI to get image for.</param>
    /// <param name="expiration">How long should the file persist in the cache.</param>
    /// <param name="refreshKey">If enabled, refreshes the key on successful cache hit, extending the lifetime by expiration.</param>
    /// <param name="token">Token that can be used to cancel the operation.</param>
    public async ValueTask<byte[]> GetOrDownloadFileFromUrlBrotli(Uri uri, TimeSpan expiration, bool refreshKey = false, CancellationToken token = default) => await GetOrDownloadFileFromUrlBrotli(uri, uri.ToString(), expiration, refreshKey, token);

    /// <summary>
    /// Retrieves a file for a given URI, with custom key.
    /// Uses Brotli compression.
    /// </summary>
    /// <param name="uri">The URI to get image for.</param>
    /// <param name="key">The key to use for the URI.</param>
    /// <param name="expiration">How long should the file persist in the cache.</param>
    /// <param name="refreshKey">If enabled, refreshes the key on successful cache hit, extending the lifetime by expiration.</param>
    /// <param name="token">Token that can be used to cancel the operation.</param>
    public async ValueTask<byte[]> GetOrDownloadFileFromUrlBrotli(Uri uri, string key, TimeSpan expiration, bool refreshKey = false, CancellationToken token = default)
    {
        // Check the cache.
        var file = await GetExistingFile(key);
        if (file != null)
        {
            if (refreshKey)
                RefreshKey(key, expiration);
            
            return Compression.DecompressToArray(file);
        }

        // Get it, and put into cache.
        var uncompressedData = await SharedHttpClient.UncachedAndCompressed.GetByteArrayAsync(uri, token);
        var data = Compression.Compress(uncompressedData); 
        if (!token.IsCancellationRequested)
            _ = _cache.Insert(key, data, expiration);

        return uncompressedData;
    }

    /// <summary>
    /// Tries to get an existing file from the database, if the file is not present, returns null.
    /// </summary>
    /// <param name="key">Key under which the file is held.</param>
    /// <returns>File if it exists, else null.</returns>
    public async Task<byte[]?> GetExistingFile(string key)
    {
        var observable = _cache.Get(key);
        return await observable.Catch(Observable.Return<byte[]?>(null!));
    }

    /// <summary>
    /// Refreshes the expiration date of a given key.
    /// </summary>
    /// <param name="key">The key to be refreshed.</param>
    /// <param name="newExpiration">New timespam of expiration relative to current.</param>
    public void RefreshKey(string key, TimeSpan newExpiration)
    {
        var expiration = _cache.Scheduler.Now + newExpiration;
        _cache.Connection.Execute($"UPDATE CacheElement SET Expiration='{expiration.Ticks}' WHERE Key='{key}'");
    }
}