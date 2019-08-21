using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Reloaded.Mod.Launcher.Converters
{
    public class FloatToThreeDecimalPlaces : IValueConverter
    {
        public static FloatToThreeDecimalPlaces Instance = new FloatToThreeDecimalPlaces();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float floatingPoint)
            {
                if (Math.Abs(floatingPoint) < 0.0001F)
                    return "N/A";

                value = floatingPoint.ToString("0.000");
                return value;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
