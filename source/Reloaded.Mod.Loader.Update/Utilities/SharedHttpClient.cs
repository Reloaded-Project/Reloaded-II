using Reloaded.Mod.Loader.Update.Caching;
using System.Net.Http;
using CacheCow.Client;

namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Provides reusable <see cref="HttpClient"/> instances.
/// </summary>
public static class SharedHttpClient
{
    /// <summary>
    /// Client with support for caching and decompression.
    /// </summary>
    public static readonly HttpClient CachedAndCompressed = AkavacheWebCacheStore.Instance.CreateClient(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.All
    });

    /// <summary>
    /// Client with support for caching.
    /// </summary>
    public static readonly HttpClient Cached = AkavacheWebCacheStore.Instance.CreateClient();

    /// <summary>
    /// Client with support for only decompression.
    /// </summary>
    public static readonly HttpClient UncachedAndCompressed = new(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.All
    });
}