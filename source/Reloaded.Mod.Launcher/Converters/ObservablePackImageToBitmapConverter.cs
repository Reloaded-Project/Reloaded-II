namespace Reloaded.Mod.Launcher.Converters;

public class ObservablePackImageToBitmapConverter : IValueConverter
{
    public static ObservablePackImageToBitmapConverter Instance { get; set; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var item = (ObservablePackImage)value;
        // This is not a bug/oversight. Photosauce doesn't run from STA threads.
        return Task.Run(() => Imaging.BitmapFromStreamViaPhotoSauce(item.Image)).Result; 
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}