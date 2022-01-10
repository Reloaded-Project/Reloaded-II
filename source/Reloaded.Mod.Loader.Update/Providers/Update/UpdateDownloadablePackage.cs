using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.Web;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Interfaces.Extensions;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Structures;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

namespace Reloaded.Mod.Loader.Update.Providers.Update;

/// <summary>
/// Represents a downloadable package that is powered internally by <see cref="Sewer56.Update"/>
/// </summary>
public class UpdateDownloadablePackage : IDownloadablePackage
{
    /// <summary>
    /// The package resovler tied to this package.
    /// </summary>
    public IPackageResolver PackageResolver { get; private set; }

    private Task _initialiseTask;

    /// <summary/>
    public UpdateDownloadablePackage(IPackageResolver packageResolver)
    {
        PackageResolver = packageResolver;
        _initialiseTask = GetPackageDetailsAsync();
    }

    /// <inheritdoc />
    public string Id { get; set; } = "";

    /// <inheritdoc />
    public string Name { get; set; } = "Unknown Package";

    /// <inheritdoc />
    public string Authors { get; set; } = "Unknown Author";

    /// <inheritdoc />
    public string Description { get; set; } = "No Description";

    /// <inheritdoc />
    public string Source { get; internal set; } = null!;

    /// <inheritdoc />
    public NuGetVersion Version { get; internal set; } = null!;

    /// <inheritdoc />
    public long FileSize { get; internal set; }

    private async Task GetPackageDetailsAsync()
    {
        var versions      = await PackageResolver.GetPackageVersionsAsync();
        Version = versions.Last();
        if (PackageResolver is IPackageResolverDownloadSize hasDownloadSize)
            FileSize = await hasDownloadSize.GetDownloadFileSizeAsync(Version, new ReleaseMetadataVerificationInfo() { FolderPath = Path.GetTempPath() });

        Source = PackageResolver.GetType().Name;
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default)
    {
        // Wait for background sync operation.
        await _initialiseTask;

        using var tempDownloadDir = new TemporaryFolderAllocation();
        using var tempExtractDir  = new TemporaryFolderAllocation();
        var tempDownloadPath      = Path.Combine(tempDownloadDir.FolderPath, Path.GetRandomFileName());

        var progressSlicer = new ProgressSlicer(progress);

        // Download Package
        var downloadSlice = progressSlicer.Slice(0.9f);
        await PackageResolver.DownloadPackageAsync(Version, tempDownloadPath, new ReleaseMetadataVerificationInfo() { FolderPath = tempDownloadPath }, downloadSlice, token);

        // Extract package.
        var extractSlice = progressSlicer.Slice(0.1f);
        var archiveExtractor = new SevenZipSharpExtractor();
        await archiveExtractor.ExtractPackageAsync(tempDownloadPath, tempExtractDir.FolderPath, extractSlice, token);

        // Copy all packages from download.
        return WebDownloadablePackage.CopyPackagesFromExtractFolderToTargetDir(packageFolder, tempExtractDir.FolderPath, token);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}