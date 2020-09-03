using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Misc
{
    public static class Constants
    {
        public const string WpfJsonFormat = "Json (*.json)|*.json";
        public const string WpfSupportedFormatsFilter = "(*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        public static string[] EmptyStringArray = new string[0];

        public const string ParameterKill = "--kill";
        public const string ParameterLaunch = "--launch";
        public const string ParameterArguments = "--arguments";
        public const string ParameterDownload  = "--download";

        public const string ReloadedProtocol = "R2";
        public const int    IcoMaxHeight = 256;
        public const int    IcoMaxWidth = 256;

        public static readonly string ApplicationPath      = Process.GetCurrentProcess().MainModule.FileName;
        public static readonly string ApplicationDirectory = Path.GetDirectoryName(ApplicationPath);

        public static Uri PlaceholderImagePath
        {
            get
            {
                var placeholder = ApplicationResourceAcquirer.GetTypeOrDefault<ImageSource>("ModPlaceholder");
                return new Uri(((BitmapFrame)placeholder).Decoder.ToString(), UriKind.Absolute);
            }
        }
    }
}
