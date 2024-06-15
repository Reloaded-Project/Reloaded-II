using Polly;
using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update.Providers.Update;

/// <summary>
/// Represents a downloadable package that is powered internally by <see cref="Sewer56.Update"/>
/// </summary>
public class UpdateDownloadablePackage : IDownloadablePackage
{
    /// <summary>
    /// The package resolver tied to this package.
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
    public string Name { get; set; } = "Unknown Package";

    /// <inheritdoc />
    public string Source { get; internal set; } = null!;

    /// <inheritdoc />
    public string? Id { get; set; }

    /// <inheritdoc />
    public string? Authors { get; set; }

    /// <inheritdoc />
    public Submitter? Submitter { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public NuGetVersion? Version { get; internal set; }

    /// <inheritdoc />
    public long? FileSize { get; internal set; }

    /// <inheritdoc />
    public string? MarkdownReadme { get; } = null!;

    /// <summary>
    /// Unsupported.
    /// </summary>
    public DownloadableImage[]? Images { get; set; } = null;

    /// <inheritdoc />
    public Uri? ProjectUri { get; set; } = null;

    /// <inheritdoc />
    public long? LikeCount { get; set; } = null;

    /// <inheritdoc />
    public long? ViewCount { get; set; } = null;

    /// <inheritdoc />
    public long? DownloadCount { get; set; } = null;

    /// <inheritdoc />
    public DateTime? Published { get; set; } = null!;

    /// <inheritdoc />
    public string? Changelog { get; }

    /// <inheritdoc />
    public string[]? Tags { get; set; }

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
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 4,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );
        
        // Download Package
        var downloadSlice = progressSlicer.Slice(0.9f);
        await retryPolicy.ExecuteAsync(async () =>
        {
            await PackageResolver.DownloadPackageAsync(Version!, tempDownloadPath, new ReleaseMetadataVerificationInfo() { FolderPath = tempDownloadPath }, downloadSlice, token);
        });

        // Extract package.
        var extractSlice = progressSlicer.Slice(0.1f);
        var archiveExtractor = new SevenZipSharpExtractor();
        await archiveExtractor.ExtractPackageAsync(tempDownloadPath, tempExtractDir.FolderPath, extractSlice, token);

        // Copy all packages from download.
        return WebDownloadablePackage.CopyPackagesFromExtractFolderToTargetDir(packageFolder, tempExtractDir.FolderPath, token);
    }

#pragma warning disable CS0067 // Event never used
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // Event never used
}