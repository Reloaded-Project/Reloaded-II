namespace Reloaded.Mod.Launcher.Converters;

/// <summary>
/// Class that converts a null or empty string to a visibility type of collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public static StringToVisibilityConverter Instance { get; set; } = new StringToVisibilityConverter();

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null or "")
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}