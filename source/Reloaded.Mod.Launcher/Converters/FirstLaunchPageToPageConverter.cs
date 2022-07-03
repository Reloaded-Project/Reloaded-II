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
                return IoC.GetConstant<Complete>();
            case FirstLaunchPage.AddApplication:
                return IoC.GetConstant<AddApplication>();
            case FirstLaunchPage.AddModExtract:
                return IoC.GetConstant<AddModExtract>();
            case FirstLaunchPage.ModEnablePage:
                return IoC.GetConstant<ModEnablePage>();
            case FirstLaunchPage.ModConfigPage:
                return IoC.GetConstant<ModConfigPage>();
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