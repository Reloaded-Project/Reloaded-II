namespace Reloaded.Mod.Loader.Update.Index.Provider;

/// <summary>
/// Package provider for NuGet that redirects requests through the index (if possible).
/// </summary>
public class IndexedNuGetPackageProvider : IDownloadablePackageProvider
{
    /// <summary>
    /// ID of the individual game.
    /// </summary>
    public string SourceUrl { get; private set; }

    private bool _initializedApi;
    private IndexPackageProvider _indexPackageProvider;
    private NuGetPackageProvider _fallback;

    /// <summary/>
    public IndexedNuGetPackageProvider(INugetRepository nugetRepository)
    {
        SourceUrl = nugetRepository.SourceUrl;
        _fallback = new NuGetPackageProvider(nugetRepository);

        _ = InitializeApiAsync();
    }

    private async Task InitializeApiAsync()
    {
        try
        {
            var indexApi = new IndexApi();
            var index = await indexApi.GetIndexAsync();
            var packages = await index.TryGetNuGetPackageList(SourceUrl);
            if (packages.result)
            {
                _indexPackageProvider = new IndexPackageProvider(packages.list);
                _initializedApi = true;
            }
        }
        catch (Exception) { /* ignored */ }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        if (!_initializedApi)
            return await _fallback.SearchAsync(text, skip, take, token);

        return await _indexPackageProvider.SearchAsync(text, skip, take, token);
    }
}

