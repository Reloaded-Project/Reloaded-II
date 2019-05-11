using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Pages;
using Page = Reloaded.Mod.Launcher.Pages.Page;

namespace Reloaded.Mod.Launcher.Converters
{
    [ValueConversion(typeof(Page), typeof(System.Windows.Controls.Page))]
    public class ApplicationPageToPageConverter : IValueConverter
    {
        public static ApplicationPageToPageConverter Instance = new ApplicationPageToPageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Page)value)
            {
                case Page.None:
                    return null;

                case Page.Base:
                    return new MainPage();

                default:
                    Debugger.Break();
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
