using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Utility;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Extractors.SevenZipSharp;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.DownloadModDialog;

/// <summary>
/// Stores the details of a mod to be downloaded.
/// </summary>
public class DownloadModDetails : ObservableObject
{
    /// <summary>
    /// The URL of an individual mod to be downloaded.
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// The file name of the mod in question.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The file size of the mod in question.
    /// </summary>
    public float FileSize { get; set; }

    /// <inheritdoc />
    public DownloadModDetails(Uri uri)
    {
        Uri = uri;

#pragma warning disable 4014
        GetNameAndSize(uri); // Fire and forget.
#pragma warning restore 4014
    }

    /// <summary>
    /// Downloads a specific file.
    /// </summary>
    public async Task DownloadAndExtract(DownloadProgressChangedEventHandler progressChanged)
    {
        using var tempDownloadDirectory = new TemporaryFolderAllocation();
        using var tempExtractDirectory = new TemporaryFolderAllocation();
        var tempFilePath = Path.Combine(tempDownloadDirectory.FolderPath, Path.GetRandomFileName());

        // Start the modification download.
        using WebClient client = new WebClient();
        client.DownloadProgressChanged += progressChanged;
        await client.DownloadFileTaskAsync(Uri, tempFilePath).ConfigureAwait(false);

        /* Extract to Temp Directory */
        var archiveExtractor = new SevenZipSharpExtractor();
        await archiveExtractor.ExtractPackageAsync(tempFilePath, tempExtractDirectory.FolderPath);

        /* Get name of package. */
        var configs = ConfigReader<ModConfig>.ReadConfigurations(tempExtractDirectory.FolderPath, ModConfig.ConfigFileName, default, int.MaxValue);
        var loaderConfig = IoC.Get<LoaderConfig>();

        foreach (var config in configs)
        {
            string configId = config.Config.ModId;
            string configDirectory = Path.GetDirectoryName(config.Path)!;
            string targetDirectory = Path.Combine(loaderConfig.ModConfigDirectory, configId);
            IOEx.MoveDirectory(configDirectory, targetDirectory);
        }
    }

    private async Task GetNameAndSize(Uri url)
    {
        using WebClient client = new WebClient();
        // Obtain the name of the file.
        try
        {
            await client.OpenReadTaskAsync(url);

            try { FileName = GetFileName(client); }
            catch (Exception) { FileName = "???"; }

            try { FileSize = GetFileSize(client); }
            catch (Exception) { FileSize = -1; }
        }
        catch (Exception) { /* Probably shouldn't swallow this one, but will for now. */ }
    }

    private string GetFileName(WebClient client) => new ContentDisposition(client.ResponseHeaders!["Content-Disposition"]!).FileName!;

    private float GetFileSize(WebClient client) => Convert.ToInt64(client.ResponseHeaders!["Content-Length"]!) / 1000F / 1000F;
}