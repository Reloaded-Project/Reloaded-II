using System.Text.Json;
using Reloaded.Mod.Loader.Update.Utilities;

namespace Reloaded.Mod.Loader.Update.Index.Utility;

/// <summary>
/// Utility class for making web requests.
/// </summary>
public static class Web
{
    /// <summary>
    /// Downloads an item from given URL and deserializes it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri">The URL to download item from.</param>
    /// <returns>The downloaded contents.</returns>
    public static async Task<T?> DownloadAndDeserialize<T>(Uri uri)
    {
        var bytes = Compression.DecompressToMemory(await SharedHttpClient.Cached.GetStreamAsync(uri));
        return JsonSerializer.Deserialize<T>(bytes.Span);
    }
}