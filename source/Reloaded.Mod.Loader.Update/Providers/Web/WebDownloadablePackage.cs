using NuGet.Protocol;
using Polly;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

namespace Reloaded.Mod.Loader.Update.Providers.Web;

/// <summary>
/// Can be used to create a downloadable package using a web URL.
/// </summary>
public class WebDownloadablePackage : IDownloadablePackage, IDownloadablePackageGetDownloadUrl
{
#pragma warning disable CS0067 // Event never used
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // Event never used

    /// <inheritdoc />
    public string Name { get; set; } = "Unknown Package Name";

    /// <inheritdoc />
    public string Source { get; set; } = "Web URL";

    /// <inheritdoc />
    public string? Id { get; set; } = null;

    /// <inheritdoc />
    public string? Authors { get; set; } = null!;

    /// <inheritdoc />
    public Submitter? Submitter { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; } 

    /// <inheritdoc />
    public NuGetVersion? Version { get; set; }

    /// <summary>
    /// Size of the file to be downloaded.
    /// </summary>
    public long? FileSize { get; set; }

    /// <inheritdoc />
    public string? MarkdownReadme { get; set; }

    /// <summary>
    /// Not natively supported. Can be added by external sources.
    /// </summary>
    public DownloadableImage[]? Images { get; set; }

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
    public string? Changelog { get; set; } = null!;

    /// <inheritdoc />
    public string[]? Tags { get; set; }

    private Uri _url;

    /// <summary>
    /// Internal use only.
    /// </summary>
    private WebDownloadablePackage()
    {
        _url = null!;
    }

    /// <summary>
    /// Creates a downloadable package given a web URL.
    /// </summary>
    /// <param name="url">URL that can be used to download the package.</param>
    /// <param name="guessNameAndSize">Guesses the file name and size of the downloadable item.</param>
    public WebDownloadablePackage(Uri url, bool guessNameAndSize)
    {
        _url = url;
#pragma warning disable CS4014
        if (guessNameAndSize)
            GetNameAndSize(url); // Fire and forget.
#pragma warning restore CS4014
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default)
    {
        using var tempDownloadDirectory = new TemporaryFolderAllocation();
        using var tempExtractDirectory = new TemporaryFolderAllocation();
        var tempFilePath = Path.Combine(tempDownloadDirectory.FolderPath, Path.GetRandomFileName());
        var progressSlicer = new ProgressSlicer(progress);

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .WaitAndRetryAsync(
                retryCount: 4,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );

        // Start the modification download.
        using var httpClient = new HttpClient();
        var downloadProgress = progressSlicer.Slice(0.9);

        await retryPolicy.ExecuteAsync(async () =>
        {
            await using var fileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate);
            var archiveStream = await httpClient.GetStreamAsync(_url, token).ConfigureAwait(false);
            await archiveStream.CopyToAsyncEx(fileStream, 262144, downloadProgress, FileSize.GetValueOrDefault(1), token);
        });

        if (token.IsCancellationRequested)
            return string.Empty;
        
        /* Extract to Temp Directory */
        var archiveExtractor = new SevenZipSharpExtractor();
        var extractProgress = progressSlicer.Slice(0.1);
        await archiveExtractor.ExtractPackageAsync(tempFilePath, tempExtractDirectory.FolderPath, extractProgress, token);

        /* Get name of package. */
        return CopyPackagesFromExtractFolderToTargetDir(packageFolder, tempExtractDirectory.FolderPath, token);
    }

    /// <inheritdoc />
    public ValueTask<string?> GetDownloadUrlAsync() => ValueTask.FromResult(_url.ToString())!;

    /// <summary>
    /// Finds all mods in <paramref name="tempExtractDir"/> and copies them to appropriate subfolders in <paramref name="packageFolder"/>.
    /// </summary>
    /// <returns>Path to last folder copied.</returns>
    public static string CopyPackagesFromExtractFolderToTargetDir(string packageFolder, string tempExtractDir, CancellationToken token)
    {
        var configs = ConfigReader<ModConfig>.ReadConfigurations(tempExtractDir, ModConfig.ConfigFileName, token, int.MaxValue, 0);
        var returnResult = "";

        foreach (var config in configs)
        {
            string configId = config.Config.ModId;
            string configDirectory = Path.GetDirectoryName(config.Path)!;
            returnResult = Path.Combine(packageFolder, IO.Utility.IOEx.ForceValidFilePath(configId));
            try { Directory.Delete(returnResult, true); }
            catch (Exception) { }
            IOEx.MoveDirectory(configDirectory, returnResult);
        }

        return returnResult;
    }

    private async Task GetNameAndSize(Uri url)
    {
        // Obtain the name of the file.
        try
        {
            var fileReq = WebRequest.CreateHttp(url);
            using var fileResp = await fileReq.GetResponseAsync();

            try
            {
                var name = fileResp.Headers["Content-Disposition"];
                if (name != null)
                    Name = name;
            }
            catch (Exception) { /* Ignored */ }

            try { FileSize = fileResp.ContentLength; }
            catch (Exception) { FileSize = -1; }
        }
        catch (Exception) { /* Probably shouldn't swallow this one, but will for now. */ }
    }

    #region NuGet

    /// <summary>
    /// Creates a web downloadable package from a NuGet source.
    /// </summary>
    /// <param name="pkg">The search result.</param>
    /// <param name="repository">The NuGet repository to use.</param>
    /// <param name="getReadme">If true, tries to pull readme from the server.</param>
    /// <param name="getReleaseNotes">If true, tries to pull release notes from the server.</param>
    /// <param name="getExtraDataAsync">Gets the publish time, size, release notes and readme without blocking.</param>
    public static async Task<WebDownloadablePackage> FromNuGetAsync(IPackageSearchMetadata pkg,
        INugetRepository repository, bool getReadme = false, bool getReleaseNotes = false, bool getExtraDataAsync = false)
    {
        var result = new WebDownloadablePackage()
        {
            Name = !string.IsNullOrEmpty(pkg.Title) ? pkg.Title : pkg.Identity.Id,
            Source = repository.FriendlyName,
            Id = pkg.Identity.Id,
            Authors = pkg.Authors,
            Submitter = new Submitter() { UserName = pkg.Authors },
            Description = pkg.Description,
            Version = pkg.Identity.Version,
            ProjectUri = pkg.ProjectUrl,
            DownloadCount = pkg.DownloadCount,
            Tags = pkg.Tags?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
        };

        var resolver = GetNuGetUpdateResolver(pkg, repository);
        result._url = new Uri((await resolver.GetDownloadUrlAsync(pkg.Identity.Version, new ReleaseMetadataVerificationInfo(), CancellationToken.None))!);
        
        var extraDataTask = InitNuGetAsyncData(result, pkg, repository, resolver, getReadme, getReleaseNotes);
        if (!getExtraDataAsync)
            await extraDataTask;

        if (pkg.IconUrl != null)
            result.Images = new[] { new DownloadableImage() { Uri = pkg.IconUrl } };

        return result;
    }

    [SuppressMessage("ReSharper", "AsyncVoidLambda")]
    private static async Task InitNuGetAsyncData(WebDownloadablePackage package, IPackageSearchMetadata pkg,
        INugetRepository repository, NuGetUpdateResolver updateResolver, bool getReadme, bool getReleaseNotes)
    {
        bool canGetReadme = getReadme && pkg.ReadmeUrl != null;

        var tasks = new Task[2 + Convert.ToInt32(canGetReadme) + Convert.ToInt32(getReleaseNotes)];
        int currentTask = 0;

        tasks[currentTask++] = Task.Run(async () => package.Published = await InitNuGetPublishedAsync(pkg, repository));
        tasks[currentTask++] = Task.Run(async () => package.FileSize = await InitNuGetFileSizeAsync(updateResolver, pkg));
        if (canGetReadme)
            tasks[currentTask++] = Task.Run(async () => package.MarkdownReadme = await SharedHttpClient.CachedAndCompressed.GetStringAsync(pkg.ReadmeUrl));

        if (getReleaseNotes)
            tasks[currentTask++] = Task.Run(async () => package.Changelog = await InitNuGetReleaseNotes(pkg, repository));

        await Task.WhenAll(tasks);
    }
    
    private static async Task<string?> InitNuGetReleaseNotes(IPackageSearchMetadata packageSearchMetadata,
        INugetRepository repository)
    {
        var reader = await repository.DownloadNuspecReaderAsync(packageSearchMetadata.Identity);
        return reader?.GetReleaseNotes();
    }

    private static async Task<long> InitNuGetFileSizeAsync(NuGetUpdateResolver resolver, IPackageSearchMetadata res)
    {
        return await resolver.GetDownloadFileSizeAsync(res.Identity.Version, new ReleaseMetadataVerificationInfo(), CancellationToken.None);
    }

    private static async ValueTask<DateTime?> InitNuGetPublishedAsync(IPackageSearchMetadata pkg, INugetRepository repository)
    {
        var details = await repository.GetPackageDetails(pkg.Identity);
        if (details != null)
            return details.Published.GetValueOrDefault().UtcDateTime;

        return null;
    }

    private static NuGetUpdateResolver GetNuGetUpdateResolver(IPackageSearchMetadata pkg, INugetRepository repository)
    {
        Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository GetRepositoryFromKey(ICacheEntry entry) => new((string)entry.Key);

        var newRepo = ItemCache<Sewer56.Update.Resolvers.NuGet.Utilities.NugetRepository>.GetOrCreateKey(repository.SourceUrl, GetRepositoryFromKey);
        var resolverSettings = new NuGetUpdateResolverSettings(pkg.Identity.Id, newRepo);
        return new NuGetUpdateResolver(resolverSettings, new CommonPackageResolverSettings() { });
    }
    #endregion
}