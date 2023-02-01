using PhotoSauce.MagicScaler;
using PhotoSauce.NativeCodecs.Libjxl;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Converts images to JPEG XL format, with maximum 1080p resolution.
/// </summary>
public class JxlImageConverter : IModPackImageConverter
{
    private const int MaxWidth = 1920;
    private const int MaxHeight = 1080;
    
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
    public unsafe MemoryStream Convert(Span<byte> source, string extension, out string newExtension)
    {        
        var settings = new ProcessImageSettings();
        settings.TrySetEncoderFormat(ImageMimeTypes.Jxl);
        settings.EncoderOptions = EncoderOptions;
        
        var output = new MemoryStream();
        fixed (byte* firstByte = &source[0])
        {
            // TODO: This is a bit inefficient as some additional work is done to build the initial pipeline, but I can't find an API in ImageScaler just to decode and some sources (\*cough\*) GameBanana force converted to WebP which wouldn't be supported by System.Drawing.Common.
            var sourceStream = new UnmanagedMemoryStream(firstByte, source.Length);
            using var pipeline = MagicImageProcessor.BuildPipeline(sourceStream, settings);
            
            // Scale image to max width/height.
            var ratioX = (double)MaxWidth / pipeline.PixelSource.Width;
            var ratioY = (double)MaxHeight / pipeline.PixelSource.Height;
            var ratio = Math.Min(ratioX, ratioY);

            // Only scale down
            if (ratio < 1)
            {
                settings.Width = (int)(pipeline.PixelSource.Width * ratio);
                settings.Height = (int)(pipeline.PixelSource.Height * ratio);
            }

            sourceStream.Position = 0;
            MagicImageProcessor.ProcessImage(sourceStream, output, settings);
        }

        output.Position = 0;
        newExtension = ".jxl";
        return output;
    }
}