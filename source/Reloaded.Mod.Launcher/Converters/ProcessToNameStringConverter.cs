namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(Process), typeof(string))]
public class ProcessToNameStringConverter : IValueConverter
{
    public string Prefix { get; set; } = "[UNK]";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Process process)
        {
            try
            {
                return $"{Prefix} Id: {process.Id}";
            }
            catch (Exception)
            {
                return "ERROR";
            }
        }

        return "ERROR";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}