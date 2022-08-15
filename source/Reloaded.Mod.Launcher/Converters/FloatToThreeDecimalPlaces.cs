namespace Reloaded.Mod.Launcher.Converters;

public class FloatToThreeDecimalPlaces : IValueConverter
{
    public static FloatToThreeDecimalPlaces Instance = new FloatToThreeDecimalPlaces();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not float floatingPoint) 
            return value.ToString()!;

        if (Math.Abs(floatingPoint) < 0.0001F)
            return "N/A";

        value = floatingPoint.ToString("0.000");
        return value;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}