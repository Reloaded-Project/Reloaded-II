namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Interface that can be used to provide support for converting images.
/// </summary>
public interface IImageConverter
{
     /// <summary>
     /// Converts the image to new format.
     /// </summary>
     /// <param name="source">Span containing the source image to be converted.</param>
     /// <param name="extension">The extension for the image.</param>
     /// <returns>The converted image in a stream. Stream should have position 0 and end at end of file.</returns>
     public MemoryStream Convert(Span<byte> source, out string extension);
}