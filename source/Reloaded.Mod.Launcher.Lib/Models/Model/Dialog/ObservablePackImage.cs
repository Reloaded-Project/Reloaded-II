namespace Reloaded.Mod.Launcher.Lib.Models.Model.Dialog;

/// <summary>
/// Image stored inside an observable package.
/// </summary>
public class ObservablePackImage : ObservableObject
{
    /// <summary>
    /// Stream containing the image contents.
    /// </summary>
    public Stream Image { get; set; }
    
    /// <summary>
    /// Caption for the image.
    /// </summary>
    public string Caption { get; set; }    
    
    /// <summary>
    /// Creates an image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="caption">Caption for the image.</param>
    public ObservablePackImage(Stream image, string caption)
    {
        Image = image;
        Caption = caption;
    }

    /// <summary>
    /// Reads the contents of the image stream into an array.
    /// </summary>
    public byte[] ToArray()
    {
        // Read Image
        var originalPos = Image.Position;
        try
        {
            Image.Position = 0;
            var result = GC.AllocateUninitializedArray<byte>((int)Image.Length);
            Image.CopyTo(new MemoryStream(result));
            return result;
        }
        finally
        {
            Image.Position = originalPos;
        }
    }
}