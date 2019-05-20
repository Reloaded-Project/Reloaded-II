using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Models.DesignTimeModel
{
    public class ApplicationIcon
    {
        public static ApplicationIcon Instance { get; set; } = new ApplicationIcon();
        public ImageSource Application { get; set; } = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE));
    }
}
