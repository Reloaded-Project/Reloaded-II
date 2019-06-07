using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Models.DesignTimeModel
{
    public class ModIcon
    {
        public static ModIcon Instance { get; set; } = new ModIcon();
        public ImageSource Mod { get; set; } = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.Absolute));
    }
}
