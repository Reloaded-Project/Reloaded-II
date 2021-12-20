using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers.NuGet;
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

    private readonly IPackageSearchMetadata _package;
    private readonly INugetRepository _repository;

    /// <summary/>
    public NuGetDownloadablePackage(IPackageSearchMetadata package, INugetRepository repository)
    {
        _package = package;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default)
    {
        using var temporaryDirectory = new TemporaryFolderAllocation();
        var progressSlicer = new ProgressSlicer(progress);
        var downloadSlice  = progressSlicer.Slice(0.9);
        var extractSlice   = progressSlicer.Slice(0.1);

        var repository       = new Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository(_repository.SourceUrl);
        var resolverSettings = new NuGetUpdateResolverSettings(_package.Identity.Id, repository);
        var resolver         = new NuGetUpdateResolver(resolverSettings, new CommonPackageResolverSettings() { });
        
        var tempDownloadPath = Path.Combine(temporaryDirectory.FolderPath, IOEx.ForceValidFilePath(_package.Identity.Id));
        var outputFolder     = Path.Combine(packageFolder, _package.Identity.Id);

        await resolver.DownloadPackageAsync(_package.Identity.Version, tempDownloadPath, new ReleaseMetadataVerificationInfo() { FolderPath = outputFolder }, downloadSlice, token);
        await _extractor.ExtractPackageAsync(tempDownloadPath, outputFolder, extractSlice, token);

        return outputFolder;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}