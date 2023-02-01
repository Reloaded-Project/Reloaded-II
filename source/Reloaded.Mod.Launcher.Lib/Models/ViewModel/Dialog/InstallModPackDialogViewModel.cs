using Reloaded.Mod.Loader.Update.Packs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel for the window/menu responsible for installing mod packs.
/// </summary>
public class InstallModPackDialogViewModel : ObservableObject
{
    /// <summary>
    /// The reader from which dialog contents are to be sourced.
    /// </summary>
    public ReloadedPackReader Reader { get; }

    /// <summary>
    /// Contains the pack that we will be installing.
    /// </summary>
    public ReloadedPack Pack { get; set; }

    /// <summary>
    /// Contains tuples of all mods and whether the current mod should be enabled or not.
    /// </summary>
    public ObservableCollection<BooleanGenericTuple<ReloadedPackItem>> Mods { get; set; } = new();

    /// <summary>
    /// Index of current page.
    /// </summary>
    public int PageIndex { get; set; } = 0;

    // Download Page Stuff //

    /// <summary>
    /// The current download progress.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Text displayed for current downloaded item.
    /// </summary>
    public string DownloadingText { get; set; } = string.Empty;

    /// <summary>
    /// List of failures returned from the download process.
    /// </summary>
    public ObservableCollection<ReloadedPack.TryDownloadResultForItem> FaultedItems { get; set; } = new();

    /// <summary>
    /// Whether we are currently downloading anything.
    /// </summary>
    public bool IsDownloading { get; set; } = false;

    /// <summary>
    /// Whether we are currently not downloading anything.
    /// </summary>
    public bool IsNotDownloading => !IsDownloading;

    private readonly LoaderConfig _config;
    private readonly AggregateNugetRepository _nugetFeeds;

    /// <summary>
    /// Creates a ViewModel for the mod pack dialog.
    /// </summary>
    /// <param name="reader">The mod pack reader.</param>
    /// <param name="loaderConfig">Provides access to the mod loader config.</param>
    /// <param name="nugetFeeds">Provides access to user configured NuGet feeds.</param>
    public InstallModPackDialogViewModel(ReloadedPackReader reader, LoaderConfig loaderConfig, AggregateNugetRepository nugetFeeds)
    {
        _config = loaderConfig;
        _nugetFeeds = nugetFeeds;
        Reader = reader;
        Pack = Reader.Pack;
        foreach (var item in Pack.Items)
            Mods.Add(new BooleanGenericTuple<ReloadedPackItem>(true, item));
    }

    /// <summary>
    /// Asynchronously downloads all items selected by the user.
    /// </summary>
    /// <returns>All items.</returns>
    public async Task DownloadAsync()
    {
        IsDownloading = true;
        try
        {
            FaultedItems.Clear();
            var failed = new List<ReloadedPack.TryDownloadResultForItem>();
            var updaterData = new UpdaterData(_nugetFeeds.Sources.Select(x => x.SourceUrl).ToList(), new CommonPackageResolverSettings());
            if (!await Pack.TryDownloadAsync(_config.GetModConfigDirectory(), GetModsToDownload(), failed, updaterData, new Progress<double>(d => Progress = d), DownloadingItemCallback, CancellationToken.None))
                FaultedItems.AddRange(failed);
        }
        finally
        {
            IsDownloading = false;
        }
    }

    /// <summary>
    /// Returns a list of all mods to be downloaded.
    /// </summary>
    /// <returns>List of all mods to be downloaded.</returns>
    public List<ReloadedPackItem> GetModsToDownload()
    {
        return Mods.Where(x => x.Enabled).Select(x => x.Generic).ToList();
    }

    /// <summary>
    /// Formats the errors present in given collection into a string.
    /// </summary>
    public string FormatError(ObservableCollection<ReloadedPack.TryDownloadResultForItem> items)
    {
        var message = new StringBuilder();
        var currentMessage = new StringBuilder();

        foreach (var faulted in FaultedItems)
        {
            currentMessage.Clear();
            var item = faulted.Item;
            var result = faulted.Result;

            // Make message
            message.Append($"{item.Name}");
            if (!string.IsNullOrEmpty(result.FailReason))
                message.Append($", Reason: {result.FailReason}");

            if (result.Ex != null)
                message.Append($", Exception: {result.Ex.Message}, {result.Ex.StackTrace}");

            // Append message
            message.AppendLine(currentMessage.ToString());
        }

        return message.ToString();
    }

    private void DownloadingItemCallback(ReloadedPackItem packItem)
    {
        DownloadingText = string.Format(Resources.InstallModPackDownloading.Get(), packItem.Name);
    }
}