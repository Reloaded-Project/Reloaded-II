using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers.NuGet;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Represents an individual NuGet package that can be downloaded.
/// </summary>
public class NuGetDownloadablePackage : IDownloadablePackage
{
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
        var repository       = new Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository(_repository.SourceUrl);
        var resolverSettings = new NuGetUpdateResolverSettings(_package.Identity.Id, repository);
        var resolver         = new NuGetUpdateResolver(resolverSettings, new CommonPackageResolverSettings() { });
        var filePath         = Path.Combine(packageFolder, _package.Identity.Id);
        await resolver.DownloadPackageAsync(_package.Identity.Version, filePath, new ReleaseMetadataVerificationInfo() { FolderPath = filePath }, progress, token);
        return filePath;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}