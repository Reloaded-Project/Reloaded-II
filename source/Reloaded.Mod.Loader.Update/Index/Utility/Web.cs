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
        Stream compressedStream;
        if (!uri.IsFile)
            compressedStream = await SharedHttpClient.Cached.GetStreamAsync(uri);
        else
            compressedStream = new FileStream(uri.LocalPath, FileMode.Open);

        var bytes = Compression.DecompressToMemory(compressedStream);
        return JsonSerializer.Deserialize<T>(bytes.Span, Serializer.Options);
    }
}