namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Package provider which aggregates results from multiple feeds.
/// </summary>
public class AggregatePackageProvider : IDownloadablePackageProvider
{
    /// <summary>
    /// Friendly name for this package provider.
    /// </summary>
    public string FriendlyName { get; set; }

    private IDownloadablePackageProvider[] _providers;

    /// <summary/>
    public AggregatePackageProvider(IDownloadablePackageProvider[] providers, string friendlyName = "No Name")
    {
        _providers = providers;
        FriendlyName = friendlyName;
    }

    /// <inheritdoc />
    public async Task<List<IDownloadablePackage>> SearchAsync(string text, int skip, int take, CancellationToken token = default)
    {
        var results = new Task<List<IDownloadablePackage>>[_providers.Length];

        for (var x = 0; x < _providers.Length; x++)
            results[x] = _providers[x].SearchAsync(text, skip, take, token);

        await Task.WhenAll(results);
        return results.SelectMany(x => x.Result).ToList();
    }
}