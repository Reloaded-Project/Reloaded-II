using Reloaded.Mod.Loader.Update.Index;

namespace Reloaded.Mod.Loader.Update.Providers.Index;

/// <summary>
///     Resolver that uses the (<a href="https://github.com/Reloaded-Project/Reloaded-II.Index">Reloaded-II index</a>)
///     to find missing packages.
/// </summary>
/// <remarks>
///     This is a last resort and has security implications. We use this, if no other method succeeds.
/// </remarks>
public class IndexDependencyResolver : IDependencyResolver
{
    private PackageList _packages;
    
    /// <summary/>
    public IndexDependencyResolver()
    {
        Task.Run(async () =>
        {
            var indexApi = new IndexApi();
            _packages = await indexApi.GetAllDependenciesAsync();
        }).Wait();
    }

    /// <inheritdoc />
    public Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default)
    {
        // Note: PackageList is a flat list of all packages.
        //       A single package may appear multiple times.
        //       Submit the highest version.
        var matchingPackages = new List<Package>();
        foreach (var package in _packages.Packages)
        {
            if (package.Id == packageId)
                matchingPackages.Add(package);
        }

        var result = new ModDependencyResolveResult();
        if (matchingPackages.Count > 0)
            result.FoundDependencies.Add(matchingPackages.OrderByDescending(x => x.Version).First());
        else
            result.NotFoundDependencies.Add(packageId);

        return Task.FromResult(result);
    }
}