using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;
using NugetRepository = Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository;

namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Represents an individual NuGet package that can be downloaded.
/// </summary>
public class NuGetDownloadablePackage : IDownloadablePackage
{
    private static NuGetPackageExtractor _extractor = new();

    /// <inheritdoc />
    public string Id => _package.Identity.Id;

    /// <inheritdoc />
    public string Name => !string.IsNullOrEmpty(_package.Title) ? _package.Title : _package.Identity.Id;

    /// <inheritdoc />
    public string Authors => _package.Authors;

    /// <summary>
    /// The submitter to use for this package.
    /// </summary>
    public Submitter Submitter => new Submitter() { UserName = _package.Authors };

    /// <inheritdoc />
    public string Description => _package.Description;

    /// <inheritdoc />
    public string Source => _repository.FriendlyName;

    /// <inheritdoc />
    public NuGetVersion Version => _package.Identity.Version;

    /// <inheritdoc />
    [DoNotNotify]
    public long FileSize
    {
        get
        {
            // Delay file size acquisition.
            if (_fileSize == null)
            {
                var resolver = _resolver.Value;
                _fileSize = resolver.GetDownloadFileSizeAsync(_package.Identity.Version, new ReleaseMetadataVerificationInfo(), CancellationToken.None).GetAwaiter().GetResult();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSize)));
            }

            return _fileSize.GetValueOrDefault();
        }
        set => _fileSize = value;
    }

    /// <inheritdoc />
    [DoNotNotify]
    public string? MarkdownReadme { get; } = null!; // unsupported

    /// <inheritdoc />
    public DownloadableImage[]? Images { get; set; }

    private readonly IPackageSearchMetadata _package;
    private readonly INugetRepository _repository;
    private long? _fileSize;
    private Lazy<NuGetUpdateResolver> _resolver;

    /// <summary/>
    public NuGetDownloadablePackage(IPackageSearchMetadata package, INugetRepository repository)
    {
        _package    = package;
        _repository = repository;
        _resolver   = new Lazy<NuGetUpdateResolver>(GetResolver, true);
        if (_package.IconUrl != null)
            Images = new[] { new DownloadableImage() { Uri = _package.IconUrl } };
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