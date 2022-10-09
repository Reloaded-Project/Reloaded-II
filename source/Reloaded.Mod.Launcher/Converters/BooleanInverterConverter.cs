namespace Reloaded.Mod.Launcher.Converters;

public class BooleanInverterConverter : IValueConverter
{
    public static BooleanInverterConverter Instance = new BooleanInverterConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;

        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}
