namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Resolver for NuGet packages.
/// </summary>
public class NuGetDependencyResolver : IDependencyResolver
{
    private readonly AggregateNugetRepository _repository;

    /// <summary/>
    public NuGetDependencyResolver(AggregateNugetRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default)
    {
        var searchResult  = await _repository.GetPackageDetails(packageId, true, true, token);
        var newestPackage = _repository.GetNewestPackage(searchResult);
        var result        = new ModDependencyResolveResult();
        
        if (newestPackage != null)
            result.FoundDependencies.Add(await WebDownloadablePackage.FromNuGetAsync(newestPackage.Generic, newestPackage.Repository));
        else
            result.NotFoundDependencies.Add(packageId);
        
        return result;
    }
}