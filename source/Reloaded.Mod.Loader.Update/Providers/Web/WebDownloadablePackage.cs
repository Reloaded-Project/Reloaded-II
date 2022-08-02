using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

namespace Reloaded.Mod.Loader.Update.Providers.Web;

/// <summary>
/// Can be used to create a downloadable package using a web URL.
/// </summary>
public class WebDownloadablePackage : IDownloadablePackage
{
#pragma warning disable CS0067 // Event never used
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // Event never used

    /// <inheritdoc />
    public string Id { get; set; } = "Unknown Package Id";

    /// <inheritdoc />
    public string Name { get; set; } = "Unknown Package Name";

    /// <inheritdoc />
    public string Authors { get; set; } = "Unknown";

    /// <inheritdoc />
    public Submitter Submitter { get; set; } = new() { UserName = "Unknown" };

    /// <inheritdoc />
    public string Description { get; set; } = "Unknown Package. Comes from the web.";

    /// <inheritdoc />
    public string Source { get; set; } = "Web URL";

    /// <inheritdoc />
    public NuGetVersion Version { get; set; } = NuGetVersion.Parse("0.0.0");

    /// <summary>
    /// Size of the file to be downloaded.
    /// </summary>
    public long FileSize { get; set; }

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

    private Uri _url;

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
        using var tempExtractDirectory  = new TemporaryFolderAllocation();
        var tempFilePath                = Path.Combine(tempDownloadDirectory.FolderPath, Path.GetRandomFileName());
        var progressSlicer              = new ProgressSlicer(progress);

        // Start the modification download.
        using WebClient client = new WebClient();
        var downloadProgress = progressSlicer.Slice(0.9);
        client.DownloadProgressChanged += (sender, args) =>
        {
            downloadProgress.Report((double)args.BytesReceived / args.TotalBytesToReceive);
        };

        await client.DownloadFileTaskAsync(_url, tempFilePath).ConfigureAwait(false);

        /* Extract to Temp Directory */
        var archiveExtractor = new SevenZipSharpExtractor();
        var extractProgress  = progressSlicer.Slice(0.1);
        await archiveExtractor.ExtractPackageAsync(tempFilePath, tempExtractDirectory.FolderPath, extractProgress, token);

        /* Get name of package. */
        return CopyPackagesFromExtractFolderToTargetDir(packageFolder, tempExtractDirectory.FolderPath, token);
    }

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
}