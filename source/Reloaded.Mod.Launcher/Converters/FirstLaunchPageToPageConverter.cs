using Complete = Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages.Complete;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(FirstLaunchPage), typeof(ReloadedPage))]
public class FirstLaunchPageToPageConverter : IValueConverter
{
    public static FirstLaunchPageToPageConverter Instance = new FirstLaunchPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((FirstLaunchPage)value)
        {
            case FirstLaunchPage.Complete:
                return Lib.IoC.Get<Complete>();
            case FirstLaunchPage.AddApplication:
                return Lib.IoC.Get<AddApplication>();
            case FirstLaunchPage.AddModExtract:
                return Lib.IoC.Get<AddModExtract>();
            case FirstLaunchPage.ModEnablePage:
                return Lib.IoC.Get<ModEnablePage>();
            case FirstLaunchPage.ModConfigPage:
                return Lib.IoC.Get<ModConfigPage>();
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