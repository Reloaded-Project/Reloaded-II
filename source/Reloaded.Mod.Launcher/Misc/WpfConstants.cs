namespace Reloaded.Mod.Launcher.Misc;
public static class WpfConstants
{
    public static Uri PlaceholderImagePath
    {
        get
        {
            var placeholder = ApplicationResourceAcquirer.GetTypeOrDefault<ImageSource>("ModPlaceholder");
            return new Uri(((BitmapFrame)placeholder!).Decoder.ToString(), UriKind.Absolute);
        }
    }
}