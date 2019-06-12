using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Ninject;
using Reloaded.Mod.Launcher.Pages;
using Reloaded.WPF.Theme.Default;
using Page = Reloaded.Mod.Launcher.Pages.Page;

namespace Reloaded.Mod.Launcher.Converters
{
    [ValueConversion(typeof(Page), typeof(ReloadedPage))]
    public class ApplicationPageToSingletonPageConverter : IValueConverter
    {
        public static ApplicationPageToSingletonPageConverter Instance = new ApplicationPageToSingletonPageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Page)value)
            {
                case Page.None:
                    return null;

                case Page.Splash:
                    return IoC.GetConstant<SplashPage>();

                case Page.Base:
                    return IoC.GetConstant<BasePage>();

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
