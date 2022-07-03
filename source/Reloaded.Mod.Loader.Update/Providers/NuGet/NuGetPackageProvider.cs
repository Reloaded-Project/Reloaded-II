namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Provider which gives back a list of downloadable mods using NuGet.
/// </summary>
public class NuGetPackageProvider : IDownloadablePackageProvider
{
    private readonly AggregateNugetRepository _repository;

    /// <summary/>
    public NuGetPackageProvider(AggregateNugetRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<List<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        var searchTuples = await _repository.Search(text, false, skip, take, token);
        var result = new List<IDownloadablePackage>();

        foreach (var tuple in searchTuples)
        {
            var packages    = tuple.Generic;
            var repository  = tuple.Repository;
            foreach (var package in packages)
                result.Add(new NuGetDownloadablePackage(package, repository));
        }

        return result;
    }
}