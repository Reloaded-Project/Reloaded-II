using Path = System.IO.Path;

namespace Reloaded.Mod.Launcher.Converters;

public class FilePathToFileConverter : IValueConverter
{
    public static FilePathToFileConverter Instance = new FilePathToFileConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
            return Path.GetFileName(path);

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}