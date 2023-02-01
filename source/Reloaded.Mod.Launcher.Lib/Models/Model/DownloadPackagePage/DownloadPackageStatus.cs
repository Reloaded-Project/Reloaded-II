namespace Reloaded.Mod.Launcher.Lib.Models.Model.DownloadPackagePage;

/// <summary>
/// Represents the status of a mod download.
/// </summary>
public enum DownloadPackageStatus
{
    /// <summary>
    /// Default: No download has started.
    /// </summary>
    Default,

    /// <summary>
    /// The mod has already been downloaded.
    /// </summary>
    AlreadyDownloaded,

    /// <summary>
    /// The mod is currently downloading.
    /// </summary>
    Downloading
}