using NuGet.Protocol.Plugins;
using System.Text;
using Window = System.Windows.Window;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.InstallModPackPages;

/// <summary>
/// Interaction logic for InstallModDownloadPage.xaml
/// </summary>
public partial class InstallModDownloadPage : ReloadedIIPage
{
    public InstallModPackDialogViewModel ViewModel { get; }

    public InstallModDownloadPage(InstallModPackDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void Click_GoBack(object sender, RoutedEventArgs e) => ViewModel.PageIndex -= 1;

    private async void Click_StartDownload(object sender, RoutedEventArgs e)
    {
        _ = PopulateCarouselAsync();
        await ViewModel.DownloadAsync();

        // Show errors.
        if (ViewModel.FaultedItems.Count > 0)
        {
            var messageBox = new MessageBox(Lib.Static.Resources.InstallModPackErrorDownloadFail.Get(), ViewModel.FormatError(ViewModel.FaultedItems));
            messageBox.Owner = Window.GetWindow(this);
            messageBox.ShowDialog();
        }

        // Close owning window.
        var wnd = Window.GetWindow(this);
        wnd?.Close();
    }

    async Task PopulateCarouselAsync()
    {
        var mods = ViewModel.GetModsToDownload();
        var images = mods.SelectMany(x => x.ImageFiles).ToList();
        images.Shuffle();

        var carousel = (Carousel)PreviewCarousel;
        await carousel.AddCaptionedImages(images, ViewModel.Reader);
        await PreviewCarousel.ForceUpdateCarouselIndex(0);
        PreviewCarousel.AutoRun = true;
    }
}
