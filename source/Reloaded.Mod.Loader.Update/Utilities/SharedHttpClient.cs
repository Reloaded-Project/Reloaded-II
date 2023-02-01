namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Provides reusable <see cref="HttpClient"/> instances.
/// </summary>
public static class SharedHttpClient
{
    private static HttpClient? _cachedAndCompressed;
    private static HttpClient? _cached;
    private static HttpClient? _uncachedAndCompressed;

    /// <summary>
    /// Client with support for caching and decompression.
    /// </summary>
    public static HttpClient CachedAndCompressed
    {
        get
        {
            if (_cachedAndCompressed != null)
                return _cachedAndCompressed;

            _cachedAndCompressed = AkavacheWebCacheStore.Instance.CreateClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.All
            });

            return _cachedAndCompressed;
        }
    }

    /// <summary>
    /// Client with support for caching.
    /// </summary>
    public static HttpClient Cached
    {
        get
        {
            if (_cached != null)
                return _cached;

            _cached = AkavacheWebCacheStore.Instance.CreateClient();
            return _cached;
        }
    }

    /// <summary>
    /// Client with support for only decompression.
    /// </summary>
    public static HttpClient UncachedAndCompressed
    {
        get
        {
            if (_uncachedAndCompressed != null)
                return _uncachedAndCompressed;

            _uncachedAndCompressed = new(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.All
            });

            return _uncachedAndCompressed;
        }
    }
}