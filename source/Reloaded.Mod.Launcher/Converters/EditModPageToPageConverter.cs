using Complete = Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages.Complete;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(EditModPage), typeof(ReloadedPage))]
public class EditModPageToPageConverter : IValueConverter
{
    public static EditModPageToPageConverter Instance = new EditModPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((EditModPage)value)
        {
            case EditModPage.Main:
                return Lib.IoC.Get<Main>();
            case EditModPage.Dependencies:
                return Lib.IoC.Get<Dependencies>();
            case EditModPage.Updates:
                return Lib.IoC.Get<Updates>();
            case EditModPage.Special:
                return Lib.IoC.Get<Complete>();
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