using Reloaded.Mod.Launcher.Lib.Interop;
using Reloaded.Mod.Launcher.Misc;

namespace Reloaded.Mod.Launcher.Interop;

public class IconConverter : IIconConverter
{
    public static IconConverter Instance = new IconConverter();

    public bool TryConvertToIcon(string imagePath, string iconPath) => Imaging.TryConvertToIcon(imagePath, iconPath);
}