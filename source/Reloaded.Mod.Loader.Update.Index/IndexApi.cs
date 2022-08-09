using Reloaded.Mod.Loader.Update.Index.Utility;

namespace Reloaded.Mod.Loader.Update.Index;

/// <summary>
/// Provides access to the search index.
/// </summary>
public class IndexApi
{
    /// <summary>
    /// URL to the website/host storing the files.
    /// Most likely GitHub Pages.
    /// </summary>
    public Uri IndexUrl { get; private set; }

    /// <summary>
    /// The URL of the search index.
    /// </summary>
    /// <param name="indexUrl">
    ///     URL of the website/host storing the files.
    ///     Should end on forward slash.
    /// </param>
    public IndexApi(string indexUrl = "https://reloaded-project.github.io/Reloaded-II.SearchIndex/")
    {
        IndexUrl = new Uri(indexUrl);
    }

    /// <summary>
    /// Returns the search index.
    /// </summary>
    public async Task<Structures.Index> GetIndexAsync()
    {
        var uri = new Uri(IndexUrl, Routes.Index);
        var index = await Web.DownloadAndDeserialize<Structures.Index>(uri);
        if (index == null)
            throw new Exception("Failed to download index.");

        index.BaseUrl = IndexUrl;
        return index;
    }
}