using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Loader.Update.Caching;

/// <summary>
/// Provides access to caches used throughout the library.
/// This class is mostly for internal use.
/// </summary>
public static class Caches
{
    private static AkavacheCache? _gameBananaFiles;

    /// <summary>
    /// Expiration time for release metadata files.
    /// </summary>
    public static TimeSpan ReleaseMetadataExpiration => TimeSpan.FromDays(14);

    /// <summary>
    /// This cache file contains the release manifests for GameBanana update resolver, bypassing
    /// fetching of them if given file ID is present in the database.
    ///
    /// On GameBanana, each file upload (mods, not images) has single numeric ID unique to it; we can save files of our choice here,
    /// such as release manifests to speed up queries. If a file is reuploaded, the ID changes, so we can cache files and retrieve them
    /// without needing to re-query the servers.
    /// </summary>
    public static AkavacheCache GameBananaFiles
    {
        get
        {
            if (_gameBananaFiles != null)
                return _gameBananaFiles;

            _gameBananaFiles = new AkavacheCache(Paths.GameBananaManifestCachePath);
            return _gameBananaFiles;
        }
    }

    /// <summary>
    /// Shuts down the caches gracefully.
    /// </summary>
    public static void Shutdown()
    {
        _gameBananaFiles?.Shutdown();
    }
}