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
    public IndexApi(string indexUrl = "https://reloaded-project.github.io/Reloaded-II.Index/")
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
    
    /// <summary>
    /// Returns all packages from the index.
    /// </summary>
    public async Task<PackageList> GetAllPackagesAsync()
    {
        var uri = new Uri(IndexUrl, Routes.AllPackages);
        return await Web.DownloadAndDeserialize<PackageList>(uri);
    }
    
    /// <summary>
    /// Returns all packages from the index.
    /// </summary>
    public async Task<PackageList> GetAllDependenciesAsync()
    {
        var uri = new Uri(IndexUrl, Routes.AllDependencies);
        return await Web.DownloadAndDeserialize<PackageList>(uri);
    }
}

/// <summary>
/// Extensions for the Index API.
/// </summary>
public static class IndexApiExtensions
{
    /// <summary>
    /// Gets an index from local folder, or returns a dummy.
    /// </summary>
    public static async Task<Structures.Index> GetOrCreateLocalIndexAsync(this IndexApi api)
    {
        var uri = new Uri(api.IndexUrl, Routes.Index);
        if (!uri.IsFile)
            throw new ArgumentException("The index API was created with a non-local path.");

        if (!File.Exists(uri.LocalPath))
            return new Structures.Index() { BaseUrl = api.IndexUrl };

        return await api.GetIndexAsync();
    }
}