using PhotoSauce.MagicScaler;
using PhotoSauce.NativeCodecs.Libjxl;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Converts images to JPEG XL format.
/// </summary>
public class JxlImageConverter : IModPackImageConverter
{
    /// <summary>
    /// Static instance of the class.
    /// </summary>
    public static JxlImageConverter Instance => new();

    private static readonly IEncoderOptions EncoderOptions = new JxlLossyEncoderOptions(1.0f, JxlEncodeSpeed.Squirrel, JxlDecodeSpeed.Slowest);
    
    static JxlImageConverter()
    {
        CodecManager.Configure(codecs => { codecs.UseLibjxl(); });
    }
    
    /// <inheritdoc />
    public MemoryStream Convert(Span<byte> source, string extension, out string newExtension)
    {        
        var settings = new ProcessImageSettings();
        settings.TrySetEncoderFormat(ImageMimeTypes.Jxl);
        settings.EncoderOptions = EncoderOptions;
        
        var output = new MemoryStream();
        MagicImageProcessor.ProcessImage(source, output, settings);
        
        output.Position = 0;
        newExtension = ".jxl";
        return output;
    }
}