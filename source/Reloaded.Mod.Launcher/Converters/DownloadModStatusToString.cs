namespace Reloaded.Mod.Launcher.Converters;

public class DownloadModStatusToString : IValueConverter
{
    public static DownloadModStatusToString Instance { get; set; } = new DownloadModStatusToString();

    private XamlResource<string> _downloadModDefaultText        = new XamlResource<string>("DownloadModsDownload");
    private XamlResource<string> _downloadModDownloading        = new XamlResource<string>("DownloadModsDownloading");
    private XamlResource<string> _downloadModAlreadyDownloaded  = new XamlResource<string>("DownloadModsAlreadyDownloaded");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadPackageStatus status)
        {
            switch (status)
            {
                case DownloadPackageStatus.Default:
                    return _downloadModDefaultText.Get();

                case DownloadPackageStatus.Downloading:
                    return _downloadModDownloading.Get();

                default:
                    return _downloadModAlreadyDownloaded.Get();
            }
        }

        return _downloadModDefaultText.Get();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}