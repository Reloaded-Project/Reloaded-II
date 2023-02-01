namespace Reloaded.Mod.Loader.Update.Interfaces.Extensions;

/// <summary>
/// Extension for <see cref="IDownloadablePackage"/> instances that allows for retrieving a URL to a given.
/// </summary>
public interface IDownloadablePackageGetDownloadUrl
{
    /// <summary>
    /// Retrieves the raw download URL for this package.
    /// </summary>
    ValueTask<string?> GetDownloadUrlAsync();
}