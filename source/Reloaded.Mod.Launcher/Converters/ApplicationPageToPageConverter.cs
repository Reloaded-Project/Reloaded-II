using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(Page), typeof(ReloadedPage))]
public class ApplicationPageToPageConverter : IValueConverter
{
    public static ApplicationPageToPageConverter Instance = new ApplicationPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((Page) value)
        {
            case Page.ManageMods:
                return IoC.GetConstant<ManageModsPage>();
            case Page.SettingsPage:
                return IoC.GetConstant<SettingsPage>();
            case Page.Application:
                return IoC.Get<ApplicationPage>();
            case Page.DownloadMods:
                return IoC.GetConstant<DownloadPackagesPage>();
            default:
                Debugger.Break();
                return null!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}