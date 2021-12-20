using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Represents a package that can be downloaded.
/// </summary>
public interface IDownloadablePackage : INotifyPropertyChanged
{
    /// <summary>
    /// Id of the mod to be downloaded.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Name of the mod to be downloaded.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The mod authors.
    /// </summary>
    public string Authors { get; }

    /// <summary>
    /// Description of the mod.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Source of the package.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Version of the mod to download.
    /// </summary>
    public NuGetVersion Version { get; }

    /// <summary>
    /// Downloads the package in question asynchronously.
    /// </summary>
    /// <param name="packageFolder">The folder containing all the packages.</param>
    /// <param name="progress">Provides progress reporting for the download operation.</param>
    /// <param name="token">Allows you to cancel the operation.</param>
    /// <returns>Folder where the package was downloaded.</returns>
    public Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default);
}