namespace Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;

/// <summary>
/// Abstracts access to a single NuGet repository.
/// </summary>
public interface INugetRepository
{
    /// <summary>
    /// Gets the URL used to create this repository.
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// A friendly name for the repository.
    /// </summary>
    public string FriendlyName { get; }

    /// <summary>
    /// Searches for packages using a specific term.
    /// </summary>
    /// <param name="searchString">The search text to look for.</param>
    /// <param name="includePrereleases">True if to include prerelease packages, else false.</param>
    /// <param name="skip">Number of search results to skip.</param>
    /// <param name="results">The max number of results to return.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    Task<IEnumerable<IPackageSearchMetadata>> Search(string searchString, bool includePrereleases, int skip = 0, int results = 50, CancellationToken token = default);

    /// <summary>
    /// Downloads the latest version of a specified NuGet package.
    /// </summary>
    /// <param name="packageId">The package to download.</param>
    /// <param name="includeUnlisted">Allows to grab an unlisted package with the given id.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <param name="includePrerelease">Allows to grab a prerelease package with the supplied id.</param>
    Task<DownloadResourceResult> DownloadPackageAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default);

    /// <summary>
    /// Downloads a specified NuGet package.
    /// </summary>
    /// <param name="packageMetadata">The package to download.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    Task<DownloadResourceResult> DownloadPackageAsync(IPackageSearchMetadata packageMetadata, CancellationToken token = default);

    /// <summary>
    /// Downloads a specified NuGet .nuspec.
    /// </summary>
    /// <param name="identity">The package identity to use.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    Task<byte[]?> DownloadNuspecAsync(PackageIdentity identity, CancellationToken token = default);

    /// <summary>
    /// Retrieves the details of an individual package.
    /// </summary>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="packageId">The unique ID of the package.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <returns>Return contains an array of versions for this package.</returns>
    Task<IEnumerable<IPackageSearchMetadata>> GetPackageDetails(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default);

    /// <summary>
    /// Retrieves the details of an individual package.
    /// </summary>
    /// <param name="identity">Contains the identity of the package (specific details)</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    /// <returns>Return contains an array of versions for this package.</returns>
    Task<IPackageSearchMetadata?> GetPackageDetails(PackageIdentity identity, CancellationToken token = default);

    /// <summary>
    /// Finds all of the dependencies of a given package, including dependencies not available in the target repository.
    /// </summary>
    /// <param name="packageId">The package Id for which to obtain the dependencies for.</param>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    Task<FindDependenciesResult> FindDependencies(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken token = default);

    /// <summary>
    /// Finds all of the dependencies of a given package, including dependencies not available in the target repository.
    /// </summary>
    /// <param name="packageSearchMetadata">The package for which to obtain the dependencies for.</param>
    /// <param name="includeUnlisted">Include unlisted packages.</param>
    /// <param name="includePrerelease">Include pre-release packages.</param>
    /// <param name="token">A cancellation token to allow cancellation of the task.</param>
    Task<FindDependenciesResult> FindDependencies(IPackageSearchMetadata packageSearchMetadata, bool includePrerelease, bool includeUnlisted, CancellationToken token = default);
}