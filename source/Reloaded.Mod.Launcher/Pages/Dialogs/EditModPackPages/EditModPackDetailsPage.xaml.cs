using System.Windows.Navigation;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPackPages;

/// <summary>
/// Interaction logic for EditMainPackDetailsPage.xaml
/// </summary>
public partial class EditModPackDetailsPage : ReloadedIIPage, IDisposable
{
    public EditModPackDetailsPageViewModel ViewModel { get; set; }

    private bool _isDisposed;

    public EditModPackDetailsPage(EditModPackDetailsPageViewModel viewModel)
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

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
    {
        ProcessExtensions.OpenHyperlink(e.Parameter.ToString()!);
        e.Handled = true;
    }

    private void SaveReadme_Click(object sender, RoutedEventArgs e) => ViewModel.SaveReadme();

    private void SetReadme_Click(object sender, RoutedEventArgs e) => ViewModel.SetReadme();

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveImageAtIndex(PreviewCarousel.PageIndex);
        PreviewCarousel.UpdatePageButtons();
    }

    private async void AddImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddImage();

        // Scroll after delay.
        // This works around a bug (for now) where it doesn't scroll the item to center when an item is added
        // to the carousel. Should fix the carousel sometime though.
        await Task.Delay(333);
        var newIndex = ViewModel.PackItem.Images.Count - 1;
        if (newIndex == PreviewCarousel.PageIndex)
            PreviewCarousel.PageIndex = -1;

        PreviewCarousel.PageIndex = newIndex;
    }
}