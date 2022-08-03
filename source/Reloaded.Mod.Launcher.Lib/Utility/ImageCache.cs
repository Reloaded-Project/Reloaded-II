using System.Reactive.Linq;
using Akavache;
using Akavache.Sqlite3;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Service that provides caching support for fetching images via URL.
/// Used as a singleton.
/// </summary>
public class ImageCacheService
{
    private readonly SqlRawPersistentBlobCache _cache;

    /// <summary/>
    public ImageCacheService()
    {
        var directory = Paths.ImageCachePath;
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "Cache.db");
        _cache = new SqlRawPersistentBlobCache(filePath);
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
    /// Retrieves the image for a given URI.
    /// </summary>
    /// <param name="uri">The URI to get image for.</param>
    /// <param name="expiration">How long should the file persist in the cache.</param>
    public async ValueTask<byte[]> GetImage(Uri uri, TimeSpan expiration)
    {
        // Check the cache.
        var key = uri.ToString();
        var observable = _cache.Get(key);
        var bytes = await observable.Catch(Observable.Return<byte[]?>(null!));

        if (bytes == null)
        {
            // Get it, and put into cache.
            using var client = new WebClient();
            var data = await client.DownloadDataTaskAsync(uri);
            _ = _cache.Insert(key, data, expiration);
            return data;
        }

        return bytes;
    }

    /// <summary>
    /// Gets the time a mod preview should expire from current time.
    /// </summary>
    public TimeSpan ModPreviewExpiration => TimeSpan.FromDays(14);
}