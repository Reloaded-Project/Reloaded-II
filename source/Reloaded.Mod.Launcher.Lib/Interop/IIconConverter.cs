namespace Reloaded.Mod.Launcher.Lib.Interop;

/// <summary>
/// Interface used for providing the conversion of images to icons.
/// </summary>
public interface IIconConverter
{
    /// <summary>
    /// Converts an image to an icon.
    /// </summary>
    /// <param name="imagePath">Path of the image to convert to icon.</param>
    /// <param name="iconPath">Path of the icon to save.</param>
    public bool TryConvertToIcon(string imagePath, string iconPath);
}