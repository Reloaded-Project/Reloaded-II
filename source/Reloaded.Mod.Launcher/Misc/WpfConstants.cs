using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Misc;
public static class WpfConstants
{
    public static Uri PlaceholderImagePath
    {
        get
        {
            var placeholder = ApplicationResourceAcquirer.GetTypeOrDefault<ImageSource>("ModPlaceholder");
            return new Uri(((BitmapFrame)placeholder!).Decoder.ToString(), UriKind.Absolute);
        }
    }
}