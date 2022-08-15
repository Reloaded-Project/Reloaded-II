namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(FirstLaunchPage), typeof(ReloadedPage))]
public class FirstLaunchPageToIntConverter : IValueConverter
{
    public static FirstLaunchPageToIntConverter Instance = new FirstLaunchPageToIntConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var page = (FirstLaunchPage) value;
        return page switch
        {
            FirstLaunchPage.AddApplication => 0,
            FirstLaunchPage.AddModExtract => 1,
            FirstLaunchPage.ModConfigPage => 2,
            FirstLaunchPage.ModEnablePage => 3,
            FirstLaunchPage.Complete => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}