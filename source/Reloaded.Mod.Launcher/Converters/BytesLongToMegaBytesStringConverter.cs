namespace Reloaded.Mod.Launcher.Converters;

public class BytesLongToMegaBytesStringConverter : IValueConverter
{
    public static BytesLongToMegaBytesStringConverter Instance = new BytesLongToMegaBytesStringConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        long valueLong = value switch
        {
            long valueAsLong => valueAsLong,
            ulong valueAsULong => (long)valueAsULong,
            int valueAsInt => valueAsInt,
            uint valueAsUInt => valueAsUInt,
            ushort valueAsUShort => valueAsUShort,
            short valueAsShort => valueAsShort,
            _ => 0
        };

        var megaBytes = valueLong / 1000.0 / 1000.0;
        if (Math.Abs(megaBytes) < 0.0001F)
            return "N/A";

        value = megaBytes.ToString("0.000");
        return value;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}