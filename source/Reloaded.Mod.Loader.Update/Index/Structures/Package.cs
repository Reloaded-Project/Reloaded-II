namespace Reloaded.Mod.Loader.Update.Index.Structures;

/// <inheritdoc />
[Equals(DoNotAddEqualityOperators = true)]
public class Package : IDownloadablePackage
{
#pragma warning disable CS0067
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

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

    /// <inheritdoc />
    public string[]? Tags { get; set; }

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

    /// <summary>
    /// Removes all information that is not needed in dependency resolution.
    /// </summary>
    public void RemoveNonDependencyInfo()
    {
        Authors = null;
        Submitter = null;
        Description = null;
        MarkdownReadme = null;
        Images = null;
        ProjectUri = null;
        LikeCount = null;
        ViewCount = null;
        DownloadCount = null;
        Published = null;
        Changelog = null;
        Tags = null;
    }
}