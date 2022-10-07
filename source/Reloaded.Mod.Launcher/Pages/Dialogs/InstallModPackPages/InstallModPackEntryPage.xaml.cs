using System.Windows.Navigation;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.InstallModPackPages;

/// <summary>
/// Interaction logic for InstallModPackEntryPage.xaml
/// </summary>
public partial class InstallModPackEntryPage : ReloadedIIPage, IDisposable
{
    public InstallModPackEntryPageViewModel ViewModel { get; }

    private bool _isDisposed;
    private bool _carouselLoaded;

    public InstallModPackEntryPage(InstallModPackEntryPageViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        ControllerSupport.SubscribePreviewCustomInputs(SubscribePreviewCustomInputs);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        ControllerSupport.UnsubscribePreviewCustomInputs(SubscribePreviewCustomInputs);
        PreviewCarousel?.Dispose();
    }

    private async void OnCarouselLoaded(object sender, RoutedEventArgs e)
    {
        if (_isDisposed || _carouselLoaded)
            return;

        _carouselLoaded = true;
        var package = ViewModel.Pack;
        var carousel = (Carousel)sender;
        var images = package.ImageFiles;
        await carousel.AddCaptionedImages(images, ViewModel.Owner.Reader);
        await PreviewCarousel.ForceUpdateCarouselIndex(0);
    }

    private void SubscribePreviewCustomInputs(in ControllerState state, ref bool handled) => PreviewCarousel.HandleCarouselImageScrollOnController(state, ref handled);

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) => ThemeHelpers.OpenHyperlink(sender, e);

    private void Start_Click(object sender, RoutedEventArgs e) => ViewModel.SetPage(1);
}