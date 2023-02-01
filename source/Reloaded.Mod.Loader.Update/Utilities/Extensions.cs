namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Miscellaneous extensions for existing items used within the library.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Retrieves verification info from a mod config and path tuple.
    /// </summary>
    /// <param name="modConfig">Config and path tuple.</param>
    public static ReleaseMetadataVerificationInfo GetVerificationInfo(this PathTuple<ModConfig> modConfig)
    {
        return new ReleaseMetadataVerificationInfo()
        {
            FolderPath = Path.GetDirectoryName(modConfig.Path)!
        };
    }

    /// <summary>
    /// Downloads a nuspec for a given package.
    /// </summary>
    /// <param name="repository">The repository to use.</param>
    /// <param name="identity">The package to get nuspec for.</param>
    /// <param name="token">You can use me to cancel operation.</param>
    public static async Task<NuspecReader?> DownloadNuspecReaderAsync(this INugetRepository repository, PackageIdentity identity, CancellationToken token = default)
    {
        var nuspec = await repository.DownloadNuspecAsync(identity, token);
        return nuspec == null ? null : new NuspecReader(new MemoryStream(nuspec));
    }
}