using Index = Reloaded.Mod.Loader.Community.Config.Index;

namespace Reloaded.Mod.Loader.Community;

/// <summary>
/// Provides access to the community index.
/// </summary>
public class IndexApi
{
    /// <summary>
    /// URL to the website/host storing the files.
    /// Most likely GitHub Pages.
    /// </summary>
    public Uri IndexUrl { get; private set; }

    /// <summary>
    /// The URL of the community index.
    /// </summary>
    /// <param name="indexUrl">
    ///     URL of the website/host storing the files.
    ///     If local path, should end on forward slash.
    /// </param>
    public IndexApi(string indexUrl = "https://reloaded-project.github.io/Reloaded.Community/")
    {
        IndexUrl = new Uri(indexUrl);
    }

    /// <summary>
    /// Returns the application index.
    /// </summary>
    public async Task<Index> GetIndexAsync()
    {
        var uri = new Uri(IndexUrl, Routes.Index);
        var index = await DownloadAndDeserialize<Index>(uri);
        if (index == null)
            throw new Exception("Failed to download index");

        return index;
    }

    /// <summary>
    /// Returns an individual application profile.
    /// </summary>
    public async Task<AppItem> GetApplicationAsync(IndexAppEntry appEntry)
    {
        if (string.IsNullOrEmpty(appEntry.FilePath))
            throw new ArgumentException($"({nameof(appEntry.FilePath)}) was null or empty.");

        var uri = new Uri(IndexUrl, Routes.GetApplicationPath(appEntry.FilePath));
        var appItem = await DownloadAndDeserialize<AppItem>(uri);
        if (appItem == null)
            throw new Exception("Failed to download package index entry.");
        
        return appItem;
    }

    private static async Task<T?> DownloadAndDeserialize<T>(Uri uri)
    {
        using var webClient = new WebClient();
        var bytes = Compression.Decompress(await webClient.DownloadDataTaskAsync(uri));
        return JsonSerializer.Deserialize<T>(bytes.Span);
    }
}