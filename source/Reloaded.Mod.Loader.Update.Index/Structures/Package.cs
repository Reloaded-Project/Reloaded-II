using System.ComponentModel;
using Mapster;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Interfaces.Extensions;
using Reloaded.Mod.Loader.Update.Providers.Web;

namespace Reloaded.Mod.Loader.Update.Index.Structures;

[Equals(DoNotAddEqualityOperators = true)]
public class Package : IDownloadablePackage
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public string Name { get; set; } = "";

    /// <inheritdoc />
    public string Source { get; set; } = "";

    /// <inheritdoc />
    public string? Id { get; set; }

    /// <inheritdoc />
    public string? Authors { get; set; }

    /// <inheritdoc />
    public Submitter? Submitter { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public NuGetVersion? Version { get; set; }

    /// <inheritdoc />
    public long? FileSize { get; set; }

    /// <inheritdoc />
    public string? MarkdownReadme { get; set; }

    /// <inheritdoc />
    public DownloadableImage[]? Images { get; set; }

    /// <inheritdoc />
    public Uri? ProjectUri { get; set; }

    /// <inheritdoc />
    public long? LikeCount { get; set; }

    /// <inheritdoc />
    public long? ViewCount { get; set; }

    /// <inheritdoc />
    public long? DownloadCount { get; set; }

    /// <inheritdoc />
    public DateTime? Published { get; set; }

    /// <inheritdoc />
    public string? Changelog { get; set; }

    /// <summary>
    /// URL needed to download this package.
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Converts an existing downloadable package into a search indexed one.
    /// </summary>
    /// <param name="package">The package to convert.</param>
    public static async ValueTask<Package> CreateAsync(IDownloadablePackage package)
    {
        var pkg = new Package();
        package.Adapt(pkg);

        if (package is not IDownloadablePackageGetDownloadUrl getDownloadUrl)
            throw new Exception($"Downloadable Package needs to support {nameof(IDownloadablePackageGetDownloadUrl)}");

        pkg.DownloadUrl = await getDownloadUrl.GetDownloadUrlAsync();
        return pkg;
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default)
    {
        var webPackage = new WebDownloadablePackage(new Uri(DownloadUrl!), false);
        this.Adapt(webPackage);
        return await webPackage.DownloadAsync(packageFolder, progress, token);
    }
}