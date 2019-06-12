using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Converters
{
    [ValueConversion(typeof(BaseSubPage), typeof(ReloadedPage))]
    public class ApplicationBaseSubPageToSingletonPageConverter : IValueConverter
    {
        public static ApplicationBaseSubPageToSingletonPageConverter Instance = new ApplicationBaseSubPageToSingletonPageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((BaseSubPage) value)
            {
                case BaseSubPage.Welcome:
                    return IoC.GetConstant<WelcomePage>();
                case BaseSubPage.AddApp:
                    return IoC.GetConstant<Pages.BaseSubpages.AddAppPage>();
                case BaseSubPage.ManageMods:
                    return IoC.GetConstant<ManageModsPage>();
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
