namespace Reloaded.Mod.Launcher.Converters;

public class ObjectToIntConverter : IValueConverter
{
    public static ObjectToIntConverter Instance = new ObjectToIntConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int) value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}