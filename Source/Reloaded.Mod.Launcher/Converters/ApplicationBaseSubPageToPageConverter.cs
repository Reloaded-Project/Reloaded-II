using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Pages;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.WPF.Theme.Default;
using Page = Reloaded.Mod.Launcher.Pages.Page;

namespace Reloaded.Mod.Launcher.Converters
{
    [ValueConversion(typeof(BaseSubPage), typeof(ReloadedPage))]
    public class ApplicationBaseSubPageToPageConverter : IValueConverter
    {
        public static ApplicationBaseSubPageToPageConverter Instance = new ApplicationBaseSubPageToPageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((BaseSubPage) value)
            {

                case BaseSubPage.Welcome:
                    return new WelcomePage();
                case BaseSubPage.AddApp:
                    return new AddAppPage();
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
