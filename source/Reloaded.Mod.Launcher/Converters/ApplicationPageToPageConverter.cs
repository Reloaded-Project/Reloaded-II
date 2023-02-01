using Page = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.Page;

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
                return Lib.IoC.Get<ManageModsPage>();
            case Page.SettingsPage:
                return Lib.IoC.Get<SettingsPage>();
            case Page.Application:
                return Lib.IoC.Get<ApplicationPage>();
            case Page.DownloadMods:
                return Lib.IoC.Get<DownloadPackagesPage>();
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