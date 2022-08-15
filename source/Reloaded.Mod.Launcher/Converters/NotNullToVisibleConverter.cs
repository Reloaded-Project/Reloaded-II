namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(object), typeof(Visibility))]
public class NotNullToVisibleConverter : IValueConverter
{
    public static NotNullToVisibleConverter Instance = new();

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value != null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}