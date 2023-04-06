using StreamExtensions = NuGet.Packaging.StreamExtensions;

namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Represents an individual item contained in a Reloaded pack.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class ReloadedPackItem
{
    /// <summary>
    /// Name of the mod represented by this item.
    /// [Shown in UI]
    /// </summary>
    public string Name { get; set; } = String.Empty;
 
    /// <summary>
    /// ID of the mod contained.
    /// [Shown in UI]
    /// </summary>
    public string ModId { get; set; } = String.Empty;
    
    /// <summary>
    /// Short description of the mod. 1 sentence.
    /// </summary>
    public string Summary { get; set; } = String.Empty;

    /// <summary>
    /// Readme for this mod, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;

    /// <summary>
    /// The file name associated with the release metadata for the mod.
    /// </summary>
    public string ReleaseMetadataFileName { get; set; } = String.Empty;

    /// <summary>
    /// List of preview image files belonging to this item.
    /// May be PNG, JPEG and JXL (JPEG XL).
    /// </summary>
    public List<ReloadedPackImage> ImageFiles { get; set; } = new();

    /// <summary>
    /// Copied from <see cref="ModConfig.PluginData"/>. Contains info on how to download the mod.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; } = new();

    /// <summary>
    /// Tries to download this item to a given output folder.
    /// </summary>
    /// <param name="outFolder">The folder to download files inside.</param>
    /// <param name="updaterData">Extra data used by the updater.</param>
    /// <param name="progress">Reports the download progress from 0 to 1.0.</param>
    /// <param name="token">This token can be used to cancel the download progress.</param>
    /// <returns>True if succeeded, else false.</returns>
    public async Task<TryDownloadResult> TryDownloadAsync(string outFolder, UpdaterData updaterData, IProgress<double>? progress, CancellationToken token = default)
    {
        Directory.CreateDirectory(outFolder);
        var mod = new PathTuple<ModConfig>(Path.Combine(outFolder, ModConfig.ConfigFileName), new ModConfig()
        {
            ModId = ModId,
            PluginData = PluginData,
            ReleaseMetadataFileName = ReleaseMetadataFileName,
        });
        
        try
        {
            var resolver = PackageResolverFactory.GetResolver(mod, null, updaterData);
            if (resolver == null)
                return Fail("Could not get update resolver for mod.", null);
            
            var baseDirectory = Path.GetDirectoryName(mod.Path);
            var metadata = new ItemMetadata(NuGetVersion.Parse("0.0.0-dummy"), baseDirectory!);
            var manager = await UpdateManager<Empty>.CreateAsync(metadata, resolver, resolver.Extractor);
            var updateResult = await manager.CheckForUpdatesAsync(token);

            if (updateResult.LastVersion == null)
                return Fail("Could not determine last version. Did mod author remove updates?", null);

            if (updateResult.CanUpdate)
            {
                await manager.PrepareUpdateAsync(updateResult.LastVersion, progress, token);
                await manager.StartUpdateAsync(updateResult.LastVersion, null, null);
            }

            progress?.Report(1);
            return new TryDownloadResult(true, null, null);
        }
        catch (Exception e)
        {
            return Fail(null, e);
        }
        
        // Fails the method.
        TryDownloadResult Fail(string? reason, Exception? ex)
        {
            progress?.Report(1);
            return new TryDownloadResult(false, reason, ex);
        }
    }
    
    /// <summary>
    /// The result of trying to download this item.
    /// </summary>
    /// <param name="Success">Whether the operation succeeded.</param>
    /// <param name="FailReason">The reason why the operation failed, if known.</param>
    /// <param name="Ex">The accompanying exception, if available.</param>
    public record struct TryDownloadResult(bool Success, string? FailReason, Exception? Ex);
}