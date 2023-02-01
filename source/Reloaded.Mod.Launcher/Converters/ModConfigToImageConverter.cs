namespace Reloaded.Mod.Launcher.Converters;

public class ModConfigToImageConverter : IMultiValueConverter
{
    public static ModConfigToImageConverter Instance = new ModConfigToImageConverter();

    /// <inheritdoc />
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        // value[0]: The path & config tuple.
        // value[1]: The icon path property. (config.Config.ModIcon). This is so we can receive property changed events.
        if (value[0] is PathTuple<ModConfig> config)
            return GetImageForModConfig(config);

        return null!;
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Obtains an image to represent a given mod, either a custom one or the default placeholder.
    /// </summary>
    public ImageSource GetImageForModConfig(PathTuple<ModConfig> modConfig)
    {
        var uri = modConfig.Config.TryGetIconPath(modConfig.Path, out string iconPath) ? new Uri(iconPath, UriKind.RelativeOrAbsolute) : WpfConstants.PlaceholderImagePath;
        return Imaging.BitmapFromUri(uri);
    }
}