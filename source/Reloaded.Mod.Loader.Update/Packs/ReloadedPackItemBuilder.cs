namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Class that builds individual package items. Owned by <see cref="ReloadedPackBuilder"/>.
/// </summary>
public class ReloadedPackItemBuilder
{
    private string _modId;
    private string _name;
    private string _readme;
    private Dictionary<string, object> _pluginData;
    private List<(Stream stream, string name, string cap)> _images = new();
    private int _imageIndex = 0;
    
    /// <summary>
    /// Creates a builder given a preconfigured ModId.
    /// </summary>
    /// <param name="modId">ID of the mod in question.</param>
    public ReloadedPackItemBuilder(string modId)
    {
        _modId = modId;
    }

    /// <summary>
    /// Sets the name for this pack item.
    /// </summary>
    public ReloadedPackItemBuilder SetName(string name)
    {
        _name = name;
        return this;
    }
    
    /// <summary>
    /// Sets the markdown readme for this pack item.
    /// </summary>
    public ReloadedPackItemBuilder SetReadme(string readme)
    {
        _readme = readme;
        return this;
    }
    
    /// <summary>
    /// Sets the plugin data for this package item.
    /// This is usually just copied from <see cref="ModConfig.PluginData"/>.
    /// </summary>
    public ReloadedPackItemBuilder SetPluginData(Dictionary<string, object> pluginData)
    {
        _pluginData = pluginData;
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
        _images.Add((imageData, $"{_modId}_{_imageIndex++}{extension}", caption));
        return this;
    }

    internal void Build(ReloadedPack pack, ZipArchive archive)
    {
        var item = new ReloadedPackItem();

        item.Name = _name;
        item.Readme = _readme;
        item.ModId = _modId;
        item.PluginData = _pluginData;
        
        // Pack images.
        foreach (var image in _images)
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