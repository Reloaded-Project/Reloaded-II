using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Converters;

public class ApplicationPathTupleToImageConverter : IMultiValueConverter
{
    public static ApplicationPathTupleToImageConverter Instance = new ApplicationPathTupleToImageConverter();

    /// <inheritdoc />
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        // value[0]: The path & config tuple.
        // value[1]: The icon path property. (config.Config.AppIcon). This is so we can receive property changed events.
        if (value[0] is PathTuple<ApplicationConfig> config)
            return GetImageForAppConfig(config);

        return null;
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Obtains an image to represent a given application.
    /// The image is either a custom one or the icon of the application.
    /// </summary>
    private ImageSource GetImageForAppConfig(PathTuple<ApplicationConfig> applicationConfig)
    {
        // Check if custom icon exists.
        if (!string.IsNullOrEmpty(applicationConfig.Config.AppIcon))
        {
            if (ApplicationConfig.TryGetApplicationIcon(applicationConfig.Path, applicationConfig.Config, out var applicationIcon))
                return Misc.Imaging.BitmapFromUri(new Uri(applicationIcon, UriKind.Absolute));
        }

        // Otherwise extract new icon from executable.
        var appLocation = ApplicationConfig.GetAbsoluteAppLocation(applicationConfig);
        if (File.Exists(appLocation))
        {
            // Else make new from icon.
            using Icon ico = Icon.ExtractAssociatedIcon(appLocation);
                
            // Extract to config set location.
            BitmapSource bitmapImage = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapImage.Freeze();

            return bitmapImage;
        }

        var image = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
        image.Freeze();

        return image;
    }
}