using Reloaded.Mod.Loader.Update.Packs;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Dialog;

/// <summary>
/// Observable pack item model, intended for editing from the UI.
/// Corresponds to <see cref="ReloadedPackItem"/>.
/// </summary>
public class ObservablePackItem : ObservableObject
{
    /// <summary>
    /// Name of the mod represented by this item.
    /// [Shown in UI]
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Readme for this mod, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;
    
    /// <summary>
    /// Short description of the mod. 1 sentence.
    /// </summary>
    public string Summary { get; set; } = String.Empty;

    /// <summary>
    /// The file name associated with the release metadata for the mod.
    /// </summary>
    public string ReleaseMetadataFileName { get; set; } = string.Empty;

    /// <summary>
    /// List of preview image files belonging to this item.
    /// May be PNG, JPEG and JXL (JPEG XL).
    /// </summary>
    public ObservableCollection<ObservablePackImage> Images { get; set; } = new();

    // Non-mutable
    
    /// <summary>
    /// ID of the mod contained.
    /// [Shown in UI]
    /// </summary>
    public string ModId { get; private set; }
    
    /// <summary>
    /// Copied from <see cref="ModConfig.PluginData"/>.
    /// Contains info on how to download the mod.
    /// </summary>
    public Dictionary<string, object> PluginData { get; private set; }
    
    /// <summary>
    /// Creates a new item of an observable pack.
    /// </summary>
    /// <param name="modId">ID of the mod in question.</param>
    /// <param name="pluginData">Contains data on how to update the mod.</param>
    public ObservablePackItem(string modId, Dictionary<string, object> pluginData)
    {
        ModId = modId;
        PluginData = pluginData;
    }

    /// <summary>
    /// Creates a new item of an observable pack.
    /// </summary>
    /// <param name="builder">Builder for the individual pack item.</param>
    public ObservablePackItem(ReloadedPackItemBuilder builder)
    {
        Name = builder.Name;
        Readme = builder.Readme;
        Summary = builder.Summary;
        ReleaseMetadataFileName = builder.ReleaseMetadataFileName;

        ModId = builder.ModId;
        PluginData = builder.PluginData;
        foreach (var image in builder.Images)
            Images.Add(new ObservablePackImage(image.stream, image.cap ?? ""));
    }

    /// <summary>
    /// Maps this model onto an item builder.
    /// </summary>
    /// <param name="itemBuilder">The item builder used to build items.</param>
    /// <param name="converter">The converter used to compress images.</param>
    public void ToBuilder(ReloadedPackItemBuilder itemBuilder, IModPackImageConverter converter)
    {
        itemBuilder.SetName(Name);
        itemBuilder.SetReadme(Readme);
        itemBuilder.SetPluginData(PluginData);
        itemBuilder.SetSummary(Summary);
        itemBuilder.SetReleaseMetadataFileName(ReleaseMetadataFileName);

        foreach (var image in Images)
        {
            // Convert image.
            var converted = converter.Convert(image.ToArray(), ".orig", out var newExt);
            itemBuilder.AddImage(converted, newExt, image.Caption);
        }
    }
}