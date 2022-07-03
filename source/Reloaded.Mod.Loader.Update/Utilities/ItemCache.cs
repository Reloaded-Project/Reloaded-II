namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// A simple cache of item of type T. 
/// </summary>
public class ItemCache<T>
{
    private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(60) });

    /// <summary>
    /// Gets an item with a specified key or creates a new cache entry.
    /// </summary>
    public static T GetOrCreateKey(string key, Func<ICacheEntry, T> getDefault) => _cache.GetOrCreate(key, getDefault);
}