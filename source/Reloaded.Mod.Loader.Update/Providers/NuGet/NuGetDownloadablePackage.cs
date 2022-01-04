using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using PropertyChanged;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers.NuGet;
using Sewer56.Update.Resolvers.NuGet.Utilities;
using Sewer56.Update.Structures;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

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