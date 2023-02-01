namespace Reloaded.Mod.Launcher.Converters;

public class StringToUriConverter : IValueConverter
{
    public static StringToUriConverter Instance = new StringToUriConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new Uri((string) value, UriKind.RelativeOrAbsolute);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}