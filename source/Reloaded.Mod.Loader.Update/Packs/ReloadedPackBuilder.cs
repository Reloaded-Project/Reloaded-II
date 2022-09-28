namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Class that can be used to help build Reloaded packs.
/// Uses a fluent API.
/// </summary>
public class ReloadedPackBuilder
{
    private string _name = string.Empty;
    private string _readme = string.Empty;
    private List<(Stream stream, string name, string? cap)> _images = new();
    private int _imageIndex = 0;
    private List<ReloadedPackItemBuilder> _itemBuilders = new();

    /// <summary>
    /// Sets the name for this Reloaded pack.
    /// </summary>
    public ReloadedPackBuilder SetName(string name)
    {
        _name = name;
        return this;
    }
    
    /// <summary>
    /// Sets the markdown readme for this Reloaded pack.
    /// </summary>
    public ReloadedPackBuilder SetReadme(string readme)
    {
        _readme = readme;
        return this;
    }

    /// <summary>
    /// Adds an image to the pack.
    /// </summary>
    /// <param name="imageData">Stream containing the image data.</param>
    /// <param name="extension">Extension of the image, with the dot.</param>
    /// <param name="caption">Caption for the image.</param>
    public ReloadedPackBuilder AddImage(Stream imageData, string extension, string? caption)
    {
        _images.Add((imageData, $"Main_{_imageIndex++}{extension}", caption));
        return this;
    }

    /// <summary>
    /// Adds a mod to this package, returning a builder that allows customization of this mod.
    /// </summary>
    /// <param name="modId">ID of the mod.</param>
    /// <returns>Builder for the individual pack item.</returns>
    public ReloadedPackItemBuilder AddModItem(string modId)
    {
        var itemBuilder = new ReloadedPackItemBuilder(modId);
        _itemBuilders.Add(itemBuilder);
        return itemBuilder;
    }

    /// <summary>
    /// Creates the Reloaded package.
    /// </summary>
    /// <returns>Stream containing the final ZIP archive.</returns>
    public MemoryStream Build(out ReloadedPack package)
    {
        // Note: zips by standard should use forward slash.
        var memStream = new MemoryStream();
        using var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true);
        
        // Create Main Package
        package = new ReloadedPack();
        package.Name = _name;
        package.Readme = _readme;
        foreach (var image in _images)
        {
            package.ImageFiles.Add(new ReloadedPackImage()
            {
                Path = image.name,
                Caption = image.cap,
            });
            
            // We use no compression assuming images are already compressed well.
            var entry = archive.CreateEntry(Routes.GetImagePath(image.name), CompressionLevel.NoCompression);
            using var entryStream = entry.Open();
            image.stream.CopyTo(entryStream);
        }
        
        // Create Mod Entries
        foreach (var builder in _itemBuilders)
            builder.Build(package, archive);
        
        // But compress the config
        var configEntry = archive.CreateEntry(Routes.Config, CompressionLevel.Optimal);
        using var configEntryStream = configEntry.Open();
        var writer = new Utf8JsonWriter(configEntryStream);
        JsonSerializer.Serialize(writer, package);
        
        return memStream;
    }
}