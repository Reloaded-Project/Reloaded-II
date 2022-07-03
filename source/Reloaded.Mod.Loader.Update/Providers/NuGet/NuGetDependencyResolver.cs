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
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, ModConfig? modConfig = null, CancellationToken token = default)
    {
        var searchResult = await _repository.FindDependencies(packageId, true, true, token);
        var result       = new ModDependencyResolveResult();

        foreach (var dependency in searchResult.Dependencies)
        {
            var package    = dependency.Generic;
            var repository = dependency.Repository;
            result.FoundDependencies.Add(new NuGetDownloadablePackage(package, repository));
        }

        foreach (var notFound in searchResult.PackagesNotFound)
            result.NotFoundDependencies.Add(notFound);

        return result;
    }
}