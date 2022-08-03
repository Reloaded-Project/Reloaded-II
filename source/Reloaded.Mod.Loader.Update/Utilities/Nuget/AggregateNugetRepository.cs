namespace Reloaded.Mod.Loader.Update.Utilities.Nuget;

/// <summary>
/// Helper class which wraps multiple <see cref="NugetRepository"/>(ies) to make it easier to interact with multiple NuGet sources.
/// </summary>
public class AggregateNugetRepository
{
    /// <summary/>
    public AggregateNugetRepository(NugetFeed[] feeds) => FromFeeds(feeds);

    /// <summary/>
    public AggregateNugetRepository(INugetRepository[] sources) => Sources = sources.ToList();

    /// <summary>For Serialization.</summary>
    // ReSharper disable once UnusedMember.Global
    public AggregateNugetRepository() { }

    /// <summary>
    /// All the sources for the helper.
    /// </summary>
    public List<INugetRepository> Sources { get; private set; } = new List<INugetRepository>();

    /// <summary>
    /// Imports a set of feeds, replacing the current set of <see cref="Sources"/>.
    /// </summary>
    /// <param name="feeds">The feeds to import.</param>
    public void FromFeeds(IReadOnlyList<NugetFeed> feeds)
    {
        var aggregateRepo = new INugetRepository[feeds.Count];
        for (var x = 0; x < feeds.Count; x++)
        {
            var repository = NugetRepository.FromSourceUrl(feeds[x].URL, feeds[x].Name);
            aggregateRepo[x] = repository;
        }

        Sources = aggregateRepo.ToList();
    }

    /// <summary>
    /// Searches for packages using a specific term in all sources.
    /// </summary>
    /// <param name="searchString">The term to search packages using.</param>
    /// <param name="includePrereleases">True if to include prerelease packages, else false.</param>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="results">The max number of results to return from each source.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <returns>Tuples of the originating source and the search result.</returns>
    public async Task<List<NugetTuple<IEnumerable<IPackageSearchMetadata>>>> Search(string searchString, bool includePrereleases, int skip = 0, int results = 50, CancellationToken token = default)
    {
        var result = new List<NugetTuple<IEnumerable<IPackageSearchMetadata>>>();
        foreach (var source in Sources)
        {
            var res = await source.Search(searchString, includePrereleases, skip, results, token).ConfigureAwait(false);
            result.Add(new NugetTuple<IEnumerable<IPackageSearchMetadata>>(source, res));
        }
            
        return result;
    }

    /// <summary>
    /// Tries to retrieve the package from all sources, returns tuples of package versions and source.
    /// </summary>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="packageId">The unique ID of the package.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    ///  <returns>Tuples of the originating source and the available versions.</returns>
    public async Task<List<NugetTuple<IEnumerable<IPackageSearchMetadata>>>> GetPackageDetails(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        var result = new List<NugetTuple<IEnumerable<IPackageSearchMetadata>>>();
        foreach (var source in Sources)
        {
            var res = await source.GetPackageDetails(packageId, includePrerelease, includeUnlisted, token).ConfigureAwait(false);
            result.Add(new NugetTuple<IEnumerable<IPackageSearchMetadata>>(source, res));
        }

        return result;
    }

    /// <summary>
    /// Retrieves the details of an individual package from the source with the latest version of the package.
    /// </summary>
    /// <param name="values">List of tuples of all sources and their packages where packages are ordered oldest to newest.</param>
    /// <returns>Source with the latest version of the package and list of versions.</returns>
    public NugetTuple<IPackageSearchMetadata>? GetNewestPackage(List<NugetTuple<IEnumerable<IPackageSearchMetadata>>> values)
    {
        NugetTuple<IPackageSearchMetadata>? newestTuple = null;

        foreach (var value in values)
        {
            // Has Versions
            var lastVersionMetadata = Nuget.GetNewestVersion(value.Generic);
            if (lastVersionMetadata == null)
                continue;

            var version = lastVersionMetadata.Identity.Version;
            if (newestTuple != null && version <= newestTuple.Generic!.Identity.Version) 
                continue;

            newestTuple = new NugetTuple<IPackageSearchMetadata>(value.Repository, lastVersionMetadata);
        }

        return newestTuple;
    }

    /// <summary>
    /// Finds all of the dependencies of a given package, including dependencies not available in the source repositories.
    /// </summary>
    /// <param name="packageId">The package Id for which to obtain the dependencies for.</param>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <returns>List of all dependencies, including the source package itself.</returns>
    public async Task<AggregateFindDependenciesResult> FindDependencies(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        var packages = await GetPackageDetails(packageId, includePrerelease, includeUnlisted, token).ConfigureAwait(false);
        var newest = GetNewestPackage(packages);
        if (newest == null)
            return new AggregateFindDependenciesResult(new HashSet<NugetTuple<IPackageSearchMetadata>>(), new HashSet<string>() { packageId });
        
        var result = await FindDependencies(newest!.Generic, includePrerelease, includeUnlisted, token).ConfigureAwait(false);
        result.Dependencies.Add(newest);
        return result;
    }

    /// <summary>
    /// Finds all of the dependencies of a given package, including dependencies not available in the source repositories.
    /// </summary>
    /// <param name="packageSearchMetadata">The package for which to obtain the dependencies for.</param>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <returns>List of all dependencies.</returns>
    public async Task<AggregateFindDependenciesResult> FindDependencies(IPackageSearchMetadata packageSearchMetadata, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        var result = new AggregateFindDependenciesResult(new HashSet<NugetTuple<IPackageSearchMetadata>>(), new HashSet<string>()); 
        await FindDependenciesRecursiveAsync(packageSearchMetadata, includePrerelease, includeUnlisted, result.Dependencies, result.PackagesNotFound, token).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Finds all of the dependencies of a given package, including dependencies not available in the target repository.
    /// </summary>
    /// <param name="packageSearchMetadata">The package for which to obtain the dependencies for.</param>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="dependenciesAccumulator">A set which will contain all packages that are dependencies of the current package.</param>
    /// <param name="packagesNotFoundAccumulator">A set which will contain all dependencies of the package that were not found in the NuGet feed.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    private async Task FindDependenciesRecursiveAsync(IPackageSearchMetadata packageSearchMetadata, bool includePrerelease, bool includeUnlisted, HashSet<NugetTuple<IPackageSearchMetadata>> dependenciesAccumulator, HashSet<string> packagesNotFoundAccumulator, CancellationToken token = default)
    {
        // Check if package metadata resolved or has dependencies.
        if (packageSearchMetadata?.DependencySets == null)
            return;

        // Go over all agnostic dependency sets.
        foreach (var dependencySet in packageSearchMetadata.DependencySets)
        {
            foreach (var package in dependencySet.Packages)
            {
                var metadata = (await GetPackageDetails(package.Id, includePrerelease, includeUnlisted, token).ConfigureAwait(false));
                if (metadata.Any(x => x.Generic!.Any()))
                {
                    var latest = GetNewestPackage(metadata);
                    if (dependenciesAccumulator.Contains(latest!))
                        continue;

                    dependenciesAccumulator.Add(latest!);
                    await FindDependenciesRecursiveAsync(latest!.Generic!, includePrerelease, includeUnlisted, dependenciesAccumulator, packagesNotFoundAccumulator, token).ConfigureAwait(false);
                }
                else
                {
                    packagesNotFoundAccumulator.Add(package.Id);
                }
            }
        }
    }
}