using Humanizer;

namespace Reloaded.Mod.Launcher.Converters;

public class DateTimeToHumanConverter : IValueConverter
{
    public static DateTimeToHumanConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
            return dateTime.Humanize();

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}