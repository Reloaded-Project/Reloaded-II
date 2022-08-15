namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public static BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter(Visibility.Hidden);
    public static BooleanToVisibilityConverter InstanceCollapsed = new BooleanToVisibilityConverter(Visibility.Collapsed);

    private Visibility _hiddenState;

    public BooleanToVisibilityConverter(Visibility hiddenState)
    {
        _hiddenState = hiddenState;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (boolValue)
                return Visibility.Visible;
        }

        return _hiddenState;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}