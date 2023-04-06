namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Class that builds individual package items. Owned by <see cref="ReloadedPackBuilder"/>.
/// </summary>
public class ReloadedPackItemBuilder
{
    /// <summary>
    /// ID of the mod.
    /// </summary>
    public string ModId { get; }

    /// <summary>
    /// Name of the mod.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Readme of the mod.
    /// </summary>
    public string Readme { get; private set; } = string.Empty;

    /// <summary>
    /// Short summary of the mod.
    /// </summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>
    /// The file name associated with the release metadata for the mod.
    /// </summary>
    public string ReleaseMetadataFileName { get; set; } = string.Empty;

    /// <summary>
    /// List of images held by this item builder.
    /// </summary>
    public Dictionary<string, object> PluginData { get; private set; } = new();
    
    /// <summary>
    /// List of images held by this item builder.
    /// </summary>
    public List<(Stream stream, string name, string? cap)> Images { get; private set; } = new();

    private int _imageIndex = 0;
    
    /// <summary>
    /// Creates a builder given a preconfigured ModId.
    /// </summary>
    /// <param name="modId">ID of the mod in question.</param>
    public ReloadedPackItemBuilder(string modId)
    {
        ModId = modId;
    }

    /// <summary>
    /// Sets the name for this pack item.
    /// </summary>
    public ReloadedPackItemBuilder SetName(string name)
    {
        Name = name;
        return this;
    }
    
    /// <summary>
    /// Sets the markdown readme for this pack item.
    /// </summary>
    public ReloadedPackItemBuilder SetReadme(string readme)
    {
        Readme = readme;
        return this;
    }

    /// <summary>
    /// Sets the summary (1 line) for this pack item.
    /// </summary>
    public ReloadedPackItemBuilder SetSummary(string summary)
    {
        Summary = summary;
        return this;
    }

    /// <summary>
    /// Sets the file name associated with the release metadata for the mod.
    /// </summary>
    public ReloadedPackItemBuilder SetReleaseMetadataFileName(string releaseMetadataFileName)
    {
        ReleaseMetadataFileName = releaseMetadataFileName;
        return this;
    }

    /// <summary>
    /// Sets the plugin data for this package item.
    /// This is usually just copied from <see cref="ModConfig.PluginData"/>.
    /// </summary>
    public ReloadedPackItemBuilder SetPluginData(Dictionary<string, object> pluginData)
    {
        PluginData = pluginData;
        return this;
    }

    /// <summary>
    /// Adds an image to the pack.
    /// </summary>
    /// <param name="imageData">Stream containing the image data.</param>
    /// <param name="extension">Extension of the image, with the dot.</param>
    /// <param name="caption">Caption for the image.</param>
    public ReloadedPackItemBuilder AddImage(Stream imageData, string extension, string? caption)
    {
        Images.Add((imageData, $"{ModId}_{_imageIndex++}{extension}", caption));
        return this;
    }

    internal void Build(ReloadedPack pack, ZipArchive archive)
    {
        var item = new ReloadedPackItem();

        item.Name = Name;
        item.Readme = Readme;
        item.ModId = ModId;
        item.Summary = Summary;
        item.ReleaseMetadataFileName = ReleaseMetadataFileName;
        item.PluginData = PluginData;
        
        // Pack images.
        foreach (var image in Images)
        {
            item.ImageFiles.Add(new ReloadedPackImage()
            {
                Path = image.name,
                Caption = image.cap,
            });
            
            // We use no compression assuming images are already compressed well.
            var entry = archive.CreateEntry(Routes.GetImagePath(image.name), CompressionLevel.NoCompression);
            using var entryStream = entry.Open();
            image.stream.CopyTo(entryStream);
        }
        
        // Add this item to main pack.
        pack.Items.Add(item);
    }
}