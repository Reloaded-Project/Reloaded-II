using PhotoSauce.MagicScaler;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Converts image to bitmap format.
/// </summary>
public class BmpImageConverter : IModPackImageConverter
{
    /// <summary>
    /// Static instance of the class.
    /// </summary>
    public static BmpImageConverter Instance => new();

    /// <inheritdoc />
    public MemoryStream Convert(Span<byte> source, string originalExtension, out string newExtension)
    {
        var settings = new ProcessImageSettings();
        settings.TrySetEncoderFormat(ImageMimeTypes.Bmp);

        var output = new MemoryStream();
        MagicImageProcessor.ProcessImage(source, output, settings);

        output.Position = 0;
        newExtension = ".bmp";
        return output;
    }
}