using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.WPF.Theme.Default;

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
                case BaseSubPage.AddApp:
                    return IoC.GetConstant<Pages.BaseSubpages.AddAppPage>();
                case BaseSubPage.ManageMods:
                    return IoC.GetConstant<ManageModsPage>();
                case BaseSubPage.SettingsPage:
                    return IoC.GetConstant<SettingsPage>();
                case BaseSubPage.Application:
                    return IoC.Get<ApplicationPage>();
                case BaseSubPage.DownloadMods:
                    return IoC.GetConstant<DownloadModsPage>();
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
