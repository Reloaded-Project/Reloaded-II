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

    /// <summary>
    /// Friendly name for this package provider.
    /// </summary>
    public string FriendlyName { get; private set; }

    private bool _initializedApi;
    private IndexPackageProvider _indexPackageProvider;
    private NuGetPackageProvider _fallback;

    /// <summary/>
    public IndexedNuGetPackageProvider(INugetRepository nugetRepository, string? appId = null)
    {
        SourceUrl = nugetRepository.SourceUrl;
        FriendlyName = nugetRepository.FriendlyName;
        _fallback = new NuGetPackageProvider(nugetRepository, appId);

        _ = InitializeApiAsync(appId);
    }

    private async Task InitializeApiAsync(string? appId)
    {
        try
        {
            var indexApi = new IndexApi();
            var index = await indexApi.GetIndexAsync();
            var packages = await index.TryGetNuGetPackageList(SourceUrl);
            if (packages.result)
            {
                // Filter out by tag if app requested
                if (!string.IsNullOrEmpty(appId))
                    packages.list.Packages = packages.list.Packages.Where(x => x.Tags != null && x.Tags.Contains(appId, StringComparer.OrdinalIgnoreCase)).ToList();

                _indexPackageProvider = new IndexPackageProvider(packages.list);
                _initializedApi = true;
            }
        }
        catch (Exception) { /* ignored */ }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, SearchOptions? options = null, CancellationToken token = default)
    {
        if (!_initializedApi)
            return await _fallback.SearchAsync(text, skip, take, options, token);

        return await _indexPackageProvider.SearchAsync(text, skip, take, options, token);
    }
}