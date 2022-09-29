using PhotoSauce.MagicScaler;
using PhotoSauce.NativeCodecs.Libjxl;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Converts image to JPEG XL format.
/// </summary>
public class JxlImageConverter : IImageConverter
{
    private static readonly IEncoderOptions EncoderOptions = new JxlLosslessEncoderOptions(JxlEncodeSpeed.Squirrel, JxlDecodeSpeed.Slowest);
    
    static JxlImageConverter()
    {
        CodecManager.Configure(codecs => { codecs.UseLibjxl(); });
    }
    
    /// <inheritdoc />
    public MemoryStream Convert(Span<byte> source, out string extension)
    {        
        var settings = new ProcessImageSettings();
        settings.TrySetEncoderFormat(ImageMimeTypes.Jxl);
        settings.EncoderOptions = EncoderOptions;
        
        var output = new MemoryStream();
        MagicImageProcessor.ProcessImage(source, output, settings);
        
        output.Position = 0;
        extension = ".jxl";
        return output;
    }
}