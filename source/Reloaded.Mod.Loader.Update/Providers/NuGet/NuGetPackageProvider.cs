namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Provider which gives back a list of downloadable mods using NuGet.
/// </summary>
public class NuGetPackageProvider : IDownloadablePackageProvider
{
    private readonly INugetRepository _repository;

    /// <summary/>
    public NuGetPackageProvider(INugetRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        var searchResults = await _repository.Search(text, false, skip, take, token);
        var result = new List<IDownloadablePackage>();

        foreach (var res in searchResults)
            result.Add(new NuGetDownloadablePackage(res, _repository));

        return result;
    }
}