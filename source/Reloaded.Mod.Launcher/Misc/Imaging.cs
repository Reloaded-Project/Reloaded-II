using Reloaded.Memory.Extensions;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Reloaded.Mod.Launcher.Misc;

public class Imaging
{
    private static BitmapImage? _placeholderIcon;

    /// <summary>
    /// Converts a WPF URI into a image.
    /// </summary>
    public static BitmapImage BitmapFromUri(Uri source)
    {
        string uri = source.OriginalString.Replace("pack://siteoforigin:,,,", Constants.ApplicationDirectory);
        using var stream = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bitmapImage = BitmapFromStream(stream);
        return bitmapImage;
    }

    /// <summary>
    /// Loads an image from a given stream.
    /// </summary>
    public static BitmapImage BitmapFromStreamViaPhotoSauce(Stream stream, int? decodePixelWidth = null, int? decodePixelHeight = null)
    {
        var origPosition = stream.Position;
        try
        {
            var data = GC.AllocateUninitializedArray<byte>((int)stream.Length);
            stream.CopyTo(new MemoryStream(data));
            var memStream = BmpImageConverter.Instance.Convert(data, "", out _);
            return BitmapFromStream(memStream, decodePixelWidth, decodePixelHeight);
        }
        finally
        {
            stream.Position = origPosition;
        }
    }

    /// <summary>
    /// Loads an image from a given stream.
    /// </summary>
    public static BitmapImage BitmapFromStream(Stream stream, int? decodePixelWidth = null, int? decodePixelHeight = null)
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        
        if (decodePixelHeight.HasValue)
            bitmapImage.DecodePixelHeight = decodePixelHeight.Value;

        if (decodePixelWidth.HasValue)
            bitmapImage.DecodePixelWidth = decodePixelWidth.Value;

        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    /// <summary>
    /// Gets the icon used for placeholders in Reloaded.
    /// </summary>
    public static BitmapImage GetPlaceholderIcon()
    {
        if (_placeholderIcon != null)
            return _placeholderIcon;

        var img = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.Freeze();

        _placeholderIcon = img;
        return img;
    }

    public static Image ImageFromUri(Uri source)
    {
        string uri = source.OriginalString.Replace("pack://siteoforigin:,,,", Constants.ApplicationDirectory);
        using var stream = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Image.FromStream(stream);
    }

    /*
     * Inspired by: https://gist.github.com/darkfall/1656050
     * Originally licensed with CC-BY-SA.
     */

    /// <summary>
    /// Converts a PNG image to a icon (ico) with all supported Windows sizes.
    /// </summary>
    /// <param name="inputBitmap">The input image.</param>
    /// <param name="output">The output stream.</param>
    public unsafe static bool TryConvertToIcon(Bitmap inputBitmap, Stream output)
    {
        if (inputBitmap == null)
            return false;

        int[] sizes = { 256, 64, 48, 32, 16 };

        // Generate PNGs for all sizes and toss them in streams
        var streams = new List<MemoryStream>();
        foreach (int size in sizes)
        {
            var newBitmap = ResizeImage(inputBitmap, size, size);
            if (newBitmap == null)
                return false;

            var imageStream = new MemoryStream();
            newBitmap.Save(imageStream, ImageFormat.Png);
            streams.Add(imageStream);
        }

        using var iconWriter = new MemoryStream();
            
        // Write ICO header.
        iconWriter.Write(new IcoHeader()
        {
            ImageType = 1,
            NumberOfImages = (short) sizes.Length
        });

        // Make Image Headers
        var imageDataOffset = sizeof(IcoHeader) + (sizeof(IcoEntry) * sizes.Length);
        for (int x = 0; x < sizes.Length; x++)
        {
            iconWriter.Write(new IcoEntry()
            {
                Width = (byte) sizes[x],
                Height = (byte) sizes[x],
                BitsPerPixel = 32,
                SizeOfImageData = (int)streams[x].Length,
                OffsetOfImageData = imageDataOffset
            });

            imageDataOffset += (int)streams[x].Length;
        }

        // Write Image Data
        for (int i = 0; i < sizes.Length; i++)
        {
            iconWriter.Write(streams[i].ToArray());
            streams[i].Close();
        }

        iconWriter.Flush();
        output.Write(iconWriter.ToArray());
        return true;
    }

    /// <summary>
    /// Converts a PNG image to a icon (ico)
    /// </summary>
    /// <param name="input">The input stream</param>
    /// <param name="output">The output stream</param
    public static bool TryConvertToIcon(Stream input, Stream output)
    {
        var inputBitmap = (Bitmap) Image.FromStream(input);
        return TryConvertToIcon(inputBitmap, output);
    }

    /// <summary>
    /// Converts a PNG image to a icon (ico)
    /// </summary>
    /// <param name="inputPath">The input path.</param>
    /// <param name="outputPath">The output path.</param>
    public static bool TryConvertToIcon(string inputPath, string outputPath)
    {
        using FileStream inputStream = new FileStream(inputPath, FileMode.Open);
        using FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate);
        return TryConvertToIcon(inputStream, outputStream);
    }

    /// <summary>
    /// Converts an image to a icon (ico)
    /// </summary>
    /// <param name="inputImage">The input image.</param>
    /// <param name="outputPath">The output path.</param>
    public static bool TryConvertToIcon(Image inputImage, string outputPath)
    {
        using FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate);
        return TryConvertToIcon(new Bitmap(inputImage), outputStream);
    }


    /// <summary>
    /// Resize the image to the specified width and height.
    /// Found on stackoverflow: https://stackoverflow.com/questions/1922040/resize-an-image-c-sharp
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using var graphics = Graphics.FromImage(destImage);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var wrapMode = new ImageAttributes();
        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

        return destImage;
    }

#pragma warning disable 0649
    internal struct IcoHeader
    {
        public short Reserved;
        public short ImageType;
        public short NumberOfImages;
    }

    internal struct IcoEntry
    {
        public byte Width;
        public byte Height;
        public byte NumColors;
        public byte Reserved;
        public short ColorPlanes;
        public short BitsPerPixel;
        public int SizeOfImageData;
        public int OffsetOfImageData;
    }
#pragma warning restore 0649
}