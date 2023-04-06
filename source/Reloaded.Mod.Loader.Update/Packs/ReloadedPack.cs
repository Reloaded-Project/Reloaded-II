using static Reloaded.Mod.Loader.Update.Packs.ReloadedPackItem;

namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Represents a pack that contains a collection of mods to be installed.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class ReloadedPack : IConfig
{
    /// <summary>
    /// Name of the pack.
    /// </summary>
    public string Name { get; set; } = String.Empty;
    
    /// <summary>
    /// Readme for the pack, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;

    /// <summary>
    /// List of preview image files belonging to the pack.
    /// May be PNG, JPEG and JXL (JPEG XL).
    /// </summary>
    public List<ReloadedPackImage> ImageFiles { get; set; } = new();
    
    /// <summary>
    /// Items associated with this pack.
    /// </summary>
    public List<ReloadedPackItem> Items { get; set; } = new();

    /// <summary>
    /// Tries to download this item to a given output folder.
    /// </summary>
    /// <param name="outFolder">The folder to download files inside. Each mod will be placed in a subfolder of this folder.</param>
    /// <param name="items">The items that should be downloaded.</param>
    /// <param name="faultedItems">The list to receive that includes mods that didn't download quite right.</param>
    /// <param name="updaterData">Extra data used by the updater.</param>
    /// <param name="progress">Reports the download progress from 0 to 1.0.</param>
    /// <param name="currentItemCallback">Reports back the current item.</param>
    /// <param name="token">This token can be used to cancel the download progress.</param>
    /// <returns>True if succeeded, else false.</returns>
    public async Task<bool> TryDownloadAsync(string outFolder, List<ReloadedPackItem> items, List<TryDownloadResultForItem> faultedItems, UpdaterData updaterData, IProgress<double>? progress, Action<ReloadedPackItem> currentItemCallback, CancellationToken token = default)
    {
        var slicer = new ProgressSlicer(progress);
        var singleItemProgress = 1.0 / items.Count;
        Directory.CreateDirectory(outFolder);
        
        for (var x = 0; x < items.Count; x++)
        {
            var item  = items[x];
            var slice = slicer.Slice(singleItemProgress);
            currentItemCallback?.Invoke(item);

            if (updaterData.CommonPackageResolverSettings.MetadataFileName != item.ReleaseMetadataFileName)
            {
                updaterData.CommonPackageResolverSettings.MetadataFileName = item.ReleaseMetadataFileName;
            }
            var downloadResult = await item.TryDownloadAsync(Path.Combine(outFolder, item.ModId), updaterData, slice, token);
            
            if (!downloadResult.Success)
                faultedItems.Add(new TryDownloadResultForItem(item, downloadResult));
        }

        progress?.Report(1);
        return faultedItems.Count <= 0;
    }

    /// <summary>
    /// Tries to download this item to a given output folder.
    /// </summary>
    /// <param name="outFolder">The folder to download files inside. Each mod will be placed in a subfolder of this folder.</param>
    /// <param name="faultedItems">The list to receive that includes mods that didn't download quite right.</param>
    /// <param name="updaterData">Extra data used by the updater.</param>
    /// <param name="progress">Reports the download progress from 0 to 1.0.</param>
    /// <param name="currentItemCallback">Reports back the current item.</param>
    /// <param name="token">This token can be used to cancel the download progress.</param>
    /// <returns>True if succeeded, else false.</returns>
    public async Task<bool> TryDownloadAsync(string outFolder, List<TryDownloadResultForItem> faultedItems, UpdaterData updaterData, IProgress<double>? progress, Action<ReloadedPackItem> currentItemCallback, CancellationToken token = default) => await TryDownloadAsync(outFolder, Items, faultedItems, updaterData, progress, currentItemCallback, token);

    /// <summary>
    /// The result of trying to download a specific item.
    /// </summary>
    /// <param name="Item">The item associated with the download result.</param>
    /// <param name="Result">The download result.</param>
    public record struct TryDownloadResultForItem(ReloadedPackItem Item, TryDownloadResult Result);
}