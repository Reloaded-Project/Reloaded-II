using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Package provider which aggregates results from multiple feeds.
/// </summary>
public class AggregatePackageProvider : IDownloadablePackageProvider
{
    private IDownloadablePackageProvider[] _providers;

    /// <summary/>
    public AggregatePackageProvider(IDownloadablePackageProvider[] providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<List<IDownloadablePackage>> SearchAsync(string text, CancellationToken token = default)
    {
        var results = new Task<List<IDownloadablePackage>>[_providers.Length];

        for (var x = 0; x < _providers.Length; x++)
            results[x] = _providers[x].SearchAsync(text, token);

        await Task.WhenAll(results);
        return results.SelectMany(x => x.Result).ToList();
    }
}