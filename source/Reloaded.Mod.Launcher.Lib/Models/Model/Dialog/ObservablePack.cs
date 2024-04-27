using Reloaded.Mod.Loader.Update.Packs;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Dialog;

/// <summary>
/// Observable pack model, intended for editing from the UI.
/// Corresponds to <see cref="ReloadedPack"/>.
/// </summary>
public class ObservablePack : ObservableObject
{
    /// <summary>
    /// Name of the pack.
    /// </summary>
    public string Name { get; set; } = "Pack Name Here";
    
    /// <summary>
    /// Readme for the pack, in markdown format.
    /// </summary>
    public string Readme { get; set; } = String.Empty;

    /// <summary>
    /// Images associated with this pack item.
    /// </summary>
    public ObservableCollection<ObservablePackImage> Images { get; set; } = new();

    /// <summary>
    /// Items associated with this pack.
    /// </summary>
    public ObservableCollection<ObservablePackItem> Items { get; set; } = new();

    /// <summary/>
    public ObservablePack() { }

    /// <summary>
    /// Creates an observable pack from a provided pack reader.
    /// </summary>
    /// <param name="packReader">Reader of the pack.</param>
    public ObservablePack(ReloadedPackReader packReader)
    {
        var pack = packReader.Pack;
        Name = pack.Name;
        Readme = pack.Readme;

        // Extract Images
        foreach (var image in pack.ImageFiles)
        {
            var data = new MemoryStream(packReader.GetImage(image.Path));
            Images.Add(new ObservablePackImage(data, image.Caption ?? ""));
        }

        // Extract Contents
        foreach (var item in pack.Items)
        {
            var obItem = new ObservablePackItem(item.ModId, item.PluginData);
            obItem.Name = item.Name;
            obItem.Readme = item.Readme;
            obItem.Summary = item.Summary;
            obItem.ReleaseMetadataFileName = item.ReleaseMetadataFileName;
            foreach (var image in item.ImageFiles)
            {
                var data = new MemoryStream(packReader.GetImage(image.Path));
                obItem.Images.Add(new ObservablePackImage(data, image.Caption ?? ""));
            }

            Items.Add(obItem);
        }
    }
    
    /// <summary>
    /// Maps this model onto a builder.
    /// </summary>
    /// <param name="packBuilder">The item builder used to build items.</param>
    /// <param name="converter">The converter used to compress images.</param>
    public void ToBuilder(ReloadedPackBuilder packBuilder, IModPackImageConverter converter)
    {
        packBuilder.SetName(Name);
        packBuilder.SetReadme(Readme);
        
        // Add images.
        foreach (var image in Images)
        {
            var converted = converter.Convert(image.ToArray(), ".orig", out var newExt);
            packBuilder.AddImage(converted, newExt, image.Caption);
        }
        
        // Add child items.
        foreach (var item in Items)
        {
            var newItem = packBuilder.AddModItem(item.ModId);
            item.ToBuilder(newItem, converter);
        }
    }
}