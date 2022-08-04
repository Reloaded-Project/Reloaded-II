using System.Numerics;
using Reloaded.Mod.Loader.Update.Interfaces;
using Image = System.Windows.Controls.Image;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for DownloadModsPage.xaml
/// </summary>
public partial class DownloadPackagesPage : ReloadedIIPage
{
    public DownloadPackagesViewModel ViewModel { get; set; }

    private ImageCacheService _cacheService;

    public DownloadPackagesPage()
    {
        InitializeComponent();
        _cacheService = Lib.IoC.GetConstant<ImageCacheService>();
        ViewModel = Lib.IoC.Get<DownloadPackagesViewModel>();
    }
    
    private async void Last_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToLastPage();
    private async void Next_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToNextPage();

    /// <summary>
    /// Asynchronously loads the image for this control.
    /// </summary>
    private async void OnPreviewImageLoaded(object sender, RoutedEventArgs e)
    {
        // Set our temporary placeholder first
        var image = (Image)sender;
        image.Source = Imaging.GetPlaceholderIcon();

        // Now download the new image async.
        var package = image.DataContext as IDownloadablePackage;
        if (package?.Images == null || package.Images.Length <= 0)
            return;

        // Calculate actual rendered size.
        var dpiScale = VisualTreeHelper.GetDpi(image);
        var desiredWidth = (int)(image.DesiredSize.Width * dpiScale.DpiScaleX);

        // Select and decode appropriate image.
        var uri = package.Images[0].SelectBasedOnWidth(desiredWidth);
        await using var memoryStream = new MemoryStream(await _cacheService.GetImage(uri, _cacheService.ModPreviewExpiration));
        image.Source = Imaging.BitmapFromStream(memoryStream, desiredWidth);
    }
}