namespace Reloaded.Mod.Launcher.Converters;

public class PackageToDescriptionConverter : IValueConverter
{
    public static PackageToDescriptionConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IDownloadablePackage package)
            return package.LongDescription!;

        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}