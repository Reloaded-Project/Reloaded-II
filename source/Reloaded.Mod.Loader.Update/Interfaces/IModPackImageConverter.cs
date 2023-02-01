namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Interface that can be used to provide support for converting images for the mod pack.
/// </summary>
public interface IModPackImageConverter
{
     /// <summary>
     /// Converts the image to new format.
     /// </summary>
     /// <param name="source">Span containing the source image to be converted.</param>
     /// <param name="originalExtension">Original extension for this format.</param>
     /// <param name="newExtension">The extension for the image.</param>
     /// <returns>The converted image in a stream. Stream should have position 0 and end at end of file.</returns>
     public MemoryStream Convert(Span<byte> source, string originalExtension, out string newExtension);
}