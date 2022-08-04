namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Represents an individual package provider which delivers downloadable packages to the user.
/// </summary>
public interface IDownloadablePackageProvider
{
    /// <summary>
    /// Searches for packages matching a given term.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    public Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default);
}