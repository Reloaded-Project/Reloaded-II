using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Reloaded.Mod.Launcher.Converters
{
    [ValueConversion(typeof(Process), typeof(string))]
    public class ProcessToNameStringConverter : IValueConverter
    {
        public static ProcessToNameStringConverter Instance = new ProcessToNameStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Process process)
            {
                try
                {
                    return $"({process.Id}) {process.MainModule.ModuleName}";
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
}
