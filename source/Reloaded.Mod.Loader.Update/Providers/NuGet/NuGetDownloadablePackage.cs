using Reloaded.Mod.Loader.Update.Interfaces.Extensions;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;
using NugetRepository = Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository;

namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Represents an individual NuGet package that can be downloaded.
/// </summary>
public class NuGetDownloadablePackage : IDownloadablePackage, IDownloadablePackageGetDownloadUrl
{
    private static NuGetPackageExtractor _extractor = new();

    /// <inheritdoc />
    public string Name => !string.IsNullOrEmpty(_package.Title) ? _package.Title : _package.Identity.Id;

    /// <inheritdoc />
    public string Source => _repository.FriendlyName;

    /// <inheritdoc />
    public string? Id => _package.Identity.Id;

    /// <inheritdoc />
    public string? Authors => _package.Authors;

    /// <inheritdoc />
    public Submitter? Submitter => new Submitter() { UserName = _package.Authors };

    /// <inheritdoc />
    public string? Description => _package.Description;

    /// <inheritdoc />
    public NuGetVersion? Version => _package.Identity.Version;

    /// <inheritdoc />
    public Uri? ProjectUri => _package.ProjectUrl;

    /// <inheritdoc />
    public long? LikeCount => null;

    /// <inheritdoc />
    public long? ViewCount => null;

    /// <inheritdoc />
    public long? DownloadCount { get; private set; }

    /// <summary>
    /// Obtained asynchronously, please wait for PropertyChanged event.
    /// </summary>
    [DoNotNotify]
    public DateTime? Published { get; set; }

    /// <summary>
    /// Unsupported. Can technically be supported but requires download of .nuspec and manual parsing.
    /// </summary>
    public string? Changelog { get; }

    /// <summary>
    /// Obtained asynchronously, please wait for PropertyChanged event.
    /// </summary>
    [DoNotNotify]
    public long? FileSize { get; set; }

    /// <inheritdoc />
    [DoNotNotify]
    public string? MarkdownReadme { get; } = null!; // unsupported

    /// <inheritdoc />
    public DownloadableImage[]? Images { get; set; }

    private readonly IPackageSearchMetadata _package;
    private readonly INugetRepository _repository;
    private Lazy<NuGetUpdateResolver> _resolver;

    /// <summary/>
    public NuGetDownloadablePackage(IPackageSearchMetadata package, INugetRepository repository)
    {
        _package    = package;
        _repository = repository;
        _resolver   = new Lazy<NuGetUpdateResolver>(GetResolver, true);
        if (_package.IconUrl != null)
            Images = new[] { new DownloadableImage() { Uri = _package.IconUrl } };
        
        DownloadCount = package.DownloadCount;
        _ = InitAsyncData();
    }

    /// <summary>
    /// Tries to initialise additional async data.
    /// </summary>
    private async Task InitAsyncData()
    {
        await Task.WhenAll(new []
        {
            InitPublishedAsync(),
            InitFileSizeAsync()
        });
    }

    private async Task InitFileSizeAsync()
    {
        var resolver = _resolver.Value;
        var fileSize = await resolver.GetDownloadFileSizeAsync(_package.Identity.Version,
            new ReleaseMetadataVerificationInfo(), CancellationToken.None);
        FileSize = fileSize;
    }

    private async Task InitPublishedAsync()
    {
        var details = await _repository.GetPackageDetails(_package.Identity);
        if (details != null)
            Published = details.Published.GetValueOrDefault().UtcDateTime;
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetDownloadUrlAsync()
    {
        var resolver = _resolver.Value;
        return await resolver.GetDownloadUrlAsync(_package.Identity.Version, new ReleaseMetadataVerificationInfo(), CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default)
    {
        using var temporaryDirectory = new TemporaryFolderAllocation();
        var progressSlicer = new ProgressSlicer(progress);
        var downloadSlice  = progressSlicer.Slice(0.9);
        var extractSlice   = progressSlicer.Slice(0.1);

        var resolver         = _resolver.Value;
        var tempDownloadPath = Path.Combine(temporaryDirectory.FolderPath, IOEx.ForceValidFilePath(_package.Identity.Id));
        var outputFolder     = Path.Combine(packageFolder, _package.Identity.Id);

        await resolver.DownloadPackageAsync(_package.Identity.Version, tempDownloadPath, new ReleaseMetadataVerificationInfo() { FolderPath = outputFolder }, downloadSlice, token);
        await _extractor.ExtractPackageAsync(tempDownloadPath, outputFolder, extractSlice, token);

        return outputFolder;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private NuGetUpdateResolver GetResolver()
    {
        NugetRepository GetRepositoryFromKey(ICacheEntry entry) => new((string)entry.Key);

        var repository = ItemCache<NugetRepository>.GetOrCreateKey(_repository.SourceUrl, GetRepositoryFromKey);
        var resolverSettings = new NuGetUpdateResolverSettings(_package.Identity.Id, repository);
        return new NuGetUpdateResolver(resolverSettings, new CommonPackageResolverSettings() { });
    }
}