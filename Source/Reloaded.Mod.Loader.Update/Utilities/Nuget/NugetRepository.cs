using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Loader.Update.Utilities.Nuget
{
    /// <summary>
    /// A wrapper around an individual NuGet repository.
    /// </summary>
    public class NugetRepository : INugetRepository
    {
        private static NullLogger _nullLogger = new NullLogger();
        private static SourceCacheContext _sourceCacheContext = new SourceCacheContext();

        private PackageSource       _packageSource;
        private SourceRepository    _sourceRepository;

        private DownloadResource        _downloadResource;
        private PackageMetadataResource _packageMetadataResource;
        private PackageSearchResource   _packageSearchResource;

        /// <param name="nugetSourceUrl">Source of a specific NuGet feed such as https://api.nuget.org/v3/index.json</param>
        public static async Task<NugetRepository> FromSourceUrlAsync(string nugetSourceUrl)
        {
            var nugetHelper = new NugetRepository();
            nugetHelper._packageSource = new PackageSource(nugetSourceUrl);
            nugetHelper._sourceRepository = new SourceRepository(nugetHelper._packageSource, Repository.Provider.GetCoreV3());

            nugetHelper._downloadResource = await nugetHelper._sourceRepository.GetResourceAsync<DownloadResource>();
            nugetHelper._packageMetadataResource = await nugetHelper._sourceRepository.GetResourceAsync<PackageMetadataResource>();
            nugetHelper._packageSearchResource = await nugetHelper._sourceRepository.GetResourceAsync<PackageSearchResource>();

            return nugetHelper;
        }

        /// <summary>
        /// Searches for packages using a specific term.
        /// </summary>
        /// <param name="includePrereleases">True if to include prerelease packages, else false.</param>
        /// <param name="results">The max number of results to return.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public async Task<IEnumerable<IPackageSearchMetadata>> Search(string searchString, bool includePrereleases, int results = 50, CancellationToken token = default)
        {
            return await _packageSearchResource.SearchAsync(searchString, new SearchFilter(includePrereleases), 0, results, _nullLogger, token);
        }

        /// <summary>
        /// Downloads the latest version of a specified NuGet package.
        /// </summary>
        /// <param name="packageId">The package to download.</param>
        /// <param name="includeUnlisted">Allows to grab an unlisted package with the given id.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        /// <param name="includePrerelease">Allows to grab a prerelease package with the supplied id.</param>
        public async Task<DownloadResourceResult> DownloadPackageAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            var package = await GetPackageDetails(packageId, includePrerelease, includeUnlisted, token);
            if (package.Any())
                return await DownloadPackageAsync(package.Last(), token);

            throw new Exception("Package with given ID was not found.");
        }

        /// <summary>
        /// Downloads a specified NuGet package.
        /// </summary>
        /// <param name="packageMetadata">The package to download.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public async Task<DownloadResourceResult> DownloadPackageAsync(IPackageSearchMetadata packageMetadata, CancellationToken token = default)
        {
            var packageIdentity = new PackageIdentity(packageMetadata.Identity.Id, packageMetadata.Identity.Version);
            var downloadContext = new PackageDownloadContext(new SourceCacheContext(), Environment.CurrentDirectory, true);
            return await _downloadResource.GetDownloadResourceResultAsync(packageIdentity, downloadContext, Path.GetTempPath(), _nullLogger, token);
        }

        /// <summary>
        /// Retrieves the details of an individual package.
        /// </summary>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="packageId">The unique ID of the package.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        /// <returns>Return contains an array of versions for this package.</returns>
        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageDetails(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            return await _packageMetadataResource.GetMetadataAsync(packageId, includePrerelease, includeUnlisted, _sourceCacheContext, _nullLogger, token);
        }

        /// <summary>
        /// Finds all of the dependencies of a given package, including dependencies not available in the target repository.
        /// </summary>
        /// <param name="packageId">The package Id for which to obtain the dependencies for.</param>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public async Task<FindDependenciesResult> FindDependencies(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            var packages = await GetPackageDetails(packageId, includePrerelease, includeUnlisted, token);
            return await FindDependencies(Nuget.GetNewestVersion(packages), includePrerelease, includeUnlisted, token);
        }

        /// <summary>
        /// Finds all of the dependencies of a given package, including dependencies not available in the target repository.
        /// </summary>
        /// <param name="packageSearchMetadata">The package for which to obtain the dependencies for.</param>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public async Task<FindDependenciesResult> FindDependencies(IPackageSearchMetadata packageSearchMetadata, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            var result = new FindDependenciesResult(new HashSet<IPackageSearchMetadata>(), new HashSet<string>());
            await FindDependenciesRecursiveAsync(packageSearchMetadata, includePrerelease, includeUnlisted, result.Dependencies, result.PackagesNotFound, token);
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
                    var metadata = (await GetPackageDetails(package.Id, includePrerelease, includeUnlisted, token)).ToArray();
                    if (metadata.Any())
                    {
                        var lastVersion = Nuget.GetNewestVersion(metadata);
                        if (dependenciesAccumulator.Contains(lastVersion))
                            continue;

                        dependenciesAccumulator.Add(lastVersion);
                        await FindDependenciesRecursiveAsync(lastVersion, includePrerelease, includeUnlisted, dependenciesAccumulator, packagesNotFoundAccumulator, token);
                    }
                    else
                    {
                        packagesNotFoundAccumulator.Add(package.Id);
                    }
                }
            }
        }
    }
}
