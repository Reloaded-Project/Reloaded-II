using ApplicationSubPage = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.ApplicationSubPage;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(ApplicationSubPage), typeof(ReloadedPage))]
public class ApplicationSubPageToPageConverter : IValueConverter
{
    public static ApplicationSubPageToPageConverter Instance = new ApplicationSubPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((ApplicationSubPage)value)
        {
            case ApplicationSubPage.Null:
                return null!;
            case ApplicationSubPage.NonReloadedProcess:
                return Lib.IoC.Get<NonReloadedProcessPage>();
            case ApplicationSubPage.ReloadedProcess:
                return Lib.IoC.Get<ReloadedProcessPage>();
            case ApplicationSubPage.ApplicationSummary:
                return Lib.IoC.Get<AppSummaryPage>();
            case ApplicationSubPage.EditApplication:
                return Lib.IoC.Get<EditAppPage>();
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