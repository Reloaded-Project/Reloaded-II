using System.Windows.Navigation;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPackPages;

/// <summary>
/// Interaction logic for EditMainPackDetailsPage.xaml
/// </summary>
public partial class EditMainPackDetailsPage : ReloadedIIPage, IDisposable
{
    public EditMainPackDetailsPageViewModel ViewModel { get; set; }

    private bool _isDisposed;

    public EditMainPackDetailsPage(EditMainPackDetailsPageViewModel viewModel)
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
    }

    // Add image switch on controller.
    private void SubscribePreviewCustomInputs(in ControllerState state, ref bool handled)
    {
        PreviewCarousel.HandleCarouselImageScrollOnController(state, ref handled);
    }

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) => ThemeHelpers.OpenHyperlink(sender, e);

    private void SetReadme_Click(object sender, RoutedEventArgs e) => ViewModel.SetReadme();

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveImageAtIndex(PreviewCarousel.PageIndex);
        PreviewCarousel.UpdatePageButtons();
    }

    private async void AddImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddImage();
        await PreviewCarousel.ForceUpdateCarouselIndex(ViewModel.Pack.Images.Count - 1);
    }
}