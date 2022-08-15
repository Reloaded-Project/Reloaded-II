namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(PageBase), typeof(ReloadedPage))]
public class ApplicationPageBaseToPageConverter : IValueConverter
{
    public static ApplicationPageBaseToPageConverter Instance = new ApplicationPageBaseToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((PageBase)value)
        {
            case PageBase.None:
                return null!;

            case PageBase.Splash:
                return Lib.IoC.Get<SplashPage>();

            case PageBase.Base:
                return Lib.IoC.GetConstant<BasePage>();

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