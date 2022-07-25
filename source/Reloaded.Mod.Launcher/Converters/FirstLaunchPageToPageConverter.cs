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
                return Lib.IoC.GetConstant<Complete>();
            case FirstLaunchPage.AddApplication:
                return Lib.IoC.GetConstant<AddApplication>();
            case FirstLaunchPage.AddModExtract:
                return Lib.IoC.GetConstant<AddModExtract>();
            case FirstLaunchPage.ModEnablePage:
                return Lib.IoC.GetConstant<ModEnablePage>();
            case FirstLaunchPage.ModConfigPage:
                return Lib.IoC.GetConstant<ModConfigPage>();
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