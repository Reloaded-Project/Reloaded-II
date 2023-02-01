namespace Reloaded.Mod.Launcher.Pages.Dialogs.InstallModPackPages;

/// <summary>
/// Interaction logic for InstallModPackModPage.xaml
/// </summary>
public partial class InstallModPackModPage : ReloadedIIPage, IDisposable
{
    public InstallModPackModPageViewModel ViewModel { get; set; }

    private bool _isDisposed;
    private bool _carouselLoaded;

    public InstallModPackModPage(InstallModPackModPageViewModel viewModel)
    {
        SwappedOut += Dispose;
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
        var package = ViewModel.Item.Generic;
        var carousel = (Carousel)sender;
        var images = package.ImageFiles;
        await carousel.AddCaptionedImages(images, ViewModel.Owner.Reader);
        await PreviewCarousel.ForceUpdateCarouselIndex(0);
        PreviewCarousel.AutoRun = true;
    }

    private void SubscribePreviewCustomInputs(in ControllerState state, ref bool handled) => PreviewCarousel.HandleCarouselImageScrollOnController(state, ref handled);

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) => ThemeHelpers.OpenHyperlink(sender, e);

    private void Click_SkipMod(object sender, RoutedEventArgs e)
    {
        ViewModel.SetDisabled();
        ViewModel.NextPage();
    }

    private void Click_AddMod(object sender, RoutedEventArgs e)
    {
        ViewModel.SetEnabled();
        ViewModel.NextPage();
    }

    private void Click_GoBack(object sender, RoutedEventArgs e)
    {
        ViewModel.LastPage();
    }
}