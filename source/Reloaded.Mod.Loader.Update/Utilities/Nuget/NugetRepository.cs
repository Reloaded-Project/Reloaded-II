using System.Reflection;

namespace Reloaded.Mod.Loader.Update.Utilities.Nuget;

/// <summary>
/// A wrapper around an individual NuGet repository.
/// </summary>
public class NugetRepository : INugetRepository
{
    /// <inheritdoc />
    public string SourceUrl { get; private set; }

    /// <inheritdoc />
    public string FriendlyName { get; set; } = null!;

    private static NullLogger _nullLogger = new NullLogger();
    private static SourceCacheContext _sourceCacheContext = new SourceCacheContext();

    private PackageSource       _packageSource = null!;
    private SourceRepository    _sourceRepository = null!;

    private AsyncLazy<DownloadResource>        _downloadResource = null!;
    private AsyncLazy<PackageMetadataResource> _packageMetadataResource = null!;
    private AsyncLazy<PackageSearchResource>   _packageSearchResource = null!;
    private string? _downloadResourceUrl;

    private NugetRepository(string sourceUrl) => SourceUrl = sourceUrl;

    /// <param name="nugetSourceUrl">Source of a specific NuGet feed such as https://api.nuget.org/v3/index.json</param>
    /// <param name="name"></param>
    public static NugetRepository FromSourceUrl(string nugetSourceUrl, string name = "Unknown Repository")
    {
        var nugetHelper                 = new NugetRepository(nugetSourceUrl);
        nugetHelper.FriendlyName        = name;
        nugetHelper._packageSource      = new PackageSource(nugetSourceUrl);
        nugetHelper._sourceRepository   = new SourceRepository(nugetHelper._packageSource, Repository.Provider.GetCoreV3());
        
        nugetHelper._downloadResource        = new AsyncLazy<DownloadResource>(async () => await nugetHelper._sourceRepository.GetResourceAsync<DownloadResource>());
        nugetHelper._packageMetadataResource = new AsyncLazy<PackageMetadataResource>(async () => await nugetHelper._sourceRepository.GetResourceAsync<PackageMetadataResource>());
        nugetHelper._packageSearchResource   = new AsyncLazy<PackageSearchResource>(async () => await nugetHelper._sourceRepository.GetResourceAsync<PackageSearchResource>());

        return nugetHelper;
    }

    /// <inheritdoc />
    public async Task<byte[]?> DownloadNuspecAsync(PackageIdentity identity, CancellationToken token = default)
    {
        var resourceUrl = await GetDownloadResourceUrl();
        if (string.IsNullOrEmpty(resourceUrl))
            return null;

        var baseUrl = new Uri($"{resourceUrl}/");
        var idLowercase = identity.Id.ToLower();
        var versionLower = identity.Version.ToString().ToLower();
        var url = new Uri(baseUrl, $"{idLowercase}/{versionLower}/{idLowercase}.nuspec");
        return await SharedHttpClient.CachedAndCompressed.GetByteArrayAsync(url, token); 
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> Search(string searchString, bool includePrereleases, int skip = 0, int results = 50, CancellationToken token = default)
    {
        try
        {
            var searchResource = await _packageSearchResource;
            return await searchResource.SearchAsync(searchString, new SearchFilter(includePrereleases), skip, results, _nullLogger, token).ConfigureAwait(false);
        }
        catch (Exception) { return Array.Empty<IPackageSearchMetadata>(); }
    }

    /// <inheritdoc />
    public async Task<DownloadResourceResult> DownloadPackageAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        try
        {
            var package = await GetPackageDetails(packageId, includePrerelease, includeUnlisted, token);
            var last = package.LastOrDefault();
            if (last != null)
                return await DownloadPackageAsync(last, token).ConfigureAwait(false);

            return new DownloadResourceResult(DownloadResourceResultStatus.NotFound);
        }
        catch (Exception)
        {
            return new DownloadResourceResult(DownloadResourceResultStatus.NotFound);
        }
    }

    /// <inheritdoc />
    public async Task<DownloadResourceResult> DownloadPackageAsync(IPackageSearchMetadata packageMetadata, CancellationToken token = default)
    {
        var packageIdentity = new PackageIdentity(packageMetadata.Identity.Id, packageMetadata.Identity.Version);
        var downloadContext = new PackageDownloadContext(new SourceCacheContext(), Environment.CurrentDirectory, true);

        try
        {
            var downloadResource = await _downloadResource;
            return await downloadResource.GetDownloadResourceResultAsync(packageIdentity, downloadContext, Path.GetTempPath(), _nullLogger, token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return new DownloadResourceResult(DownloadResourceResultStatus.NotFound);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageDetails(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        try
        {
            var metadataResource = await _packageMetadataResource;
            return await metadataResource.GetMetadataAsync(packageId, includePrerelease, includeUnlisted, _sourceCacheContext, _nullLogger, token).ConfigureAwait(false);
        }
        catch (Exception) { return Array.Empty<IPackageSearchMetadata>(); }
    }

    /// <inheritdoc />
    public async Task<IPackageSearchMetadata?> GetPackageDetails(PackageIdentity identity, CancellationToken token = default)
    {
        try
        {
            var metadataResource = await _packageMetadataResource;
            return await metadataResource.GetMetadataAsync(identity, _sourceCacheContext, _nullLogger, token).ConfigureAwait(false);
        }
        catch (Exception) { return null; }
    }

    /// <inheritdoc />
    public async Task<FindDependenciesResult> FindDependencies(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        try
        {
            var packages = await GetPackageDetails(packageId, includePrerelease, includeUnlisted, token);
            return await FindDependencies(Nuget.GetNewestVersion(packages), includePrerelease, includeUnlisted, token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return new FindDependenciesResult(new HashSet<IPackageSearchMetadata>(1), new HashSet<string>(1));
        }
    }

    /// <inheritdoc />
    public async Task<FindDependenciesResult> FindDependencies(IPackageSearchMetadata? packageSearchMetadata, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
    {
        var result = new FindDependenciesResult(new HashSet<IPackageSearchMetadata>(), new HashSet<string>());
        if (packageSearchMetadata == null)
            return result;

        try
        {
            await FindDependenciesRecursiveAsync(packageSearchMetadata, includePrerelease, includeUnlisted, result.Dependencies, result.PackagesNotFound, token).ConfigureAwait(false);
            return result;
        }
        catch (Exception)
        {
            return new FindDependenciesResult(new HashSet<IPackageSearchMetadata>(1), new HashSet<string>(1));
        }
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
    private async Task FindDependenciesRecursiveAsync(IPackageSearchMetadata packageSearchMetadata, bool includePrerelease, bool includeUnlisted, HashSet<IPackageSearchMetadata> dependenciesAccumulator, HashSet<string> packagesNotFoundAccumulator, CancellationToken token = default)
    {
        // Check if package metadata resolved or has dependencies.
        if (packageSearchMetadata?.DependencySets == null)
            return;

        // Go over all agnostic dependency sets.
        foreach (var dependencySet in packageSearchMetadata.DependencySets)
        {
            foreach (var package in dependencySet.Packages)
            {
                var metadata = (await GetPackageDetails(package.Id, includePrerelease, includeUnlisted, token).ConfigureAwait(false)).ToArray();
                if (metadata.Any())
                {
                    var lastVersion = Nuget.GetNewestVersion(metadata)!;
                    if (dependenciesAccumulator.Contains(lastVersion))
                        continue;

                    dependenciesAccumulator.Add(lastVersion);
                    await FindDependenciesRecursiveAsync(lastVersion, includePrerelease, includeUnlisted, dependenciesAccumulator, packagesNotFoundAccumulator, token).ConfigureAwait(false);
                }
                else
                {
                    packagesNotFoundAccumulator.Add(package.Id);
                }
            }
        }
    }

    /// <summary>
    /// [WARNING: REFLECTION]
    /// </summary>
    private async ValueTask<string?> GetDownloadResourceUrl()
    {
        if (!string.IsNullOrEmpty(_downloadResourceUrl))
            return _downloadResourceUrl;

        try
        {
            var downloadResource = await _downloadResource;

            // No public API for this, ah shit, here we go again.
            var dynMethod = downloadResource.GetType().GetField("_packageBaseAddressUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            _downloadResourceUrl = (string?)dynMethod!.GetValue(downloadResource)!;
            return _downloadResourceUrl;
        }
        catch (Exception)
        {
            return null;
        }
    }
}