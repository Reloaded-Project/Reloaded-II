using Reloaded.Mod.Launcher.Utility;
using Button = Sewer56.UI.Controller.Core.Enums.Button;
using Image = System.Windows.Controls.Image;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for DownloadModsPage.xaml
/// </summary>
public partial class DownloadPackagesPage : ReloadedIIPage, IDisposable
{
    public DownloadPackagesViewModel ViewModel { get; set; }

    private ImageCacheService _cacheService;
    private bool _disposed;
    private CollectionViewSource _packageViewSource;
    private ModConfigService _modConfService;

    public DownloadPackagesPage(ModConfigService modConfService)
    {
        SwappedOut += Dispose;
        _modConfService = modConfService;
        InitializeComponent();

        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _packageViewSource = manipulator.Get<CollectionViewSource>("FilteredPackages");
        _packageViewSource.Filter += FilterDisplayedPackages;

        _cacheService = Lib.IoC.GetConstant<ImageCacheService>();
        ViewModel = Lib.IoC.Get<DownloadPackagesViewModel>();
        ViewModel.SelectNextItem.AfterExecute += o => OpenPackagePreviewPage(SlideDirection.Right, SlideDirection.Left);
        ViewModel.SelectLastItem.AfterExecute += o => OpenPackagePreviewPage(SlideDirection.Left, SlideDirection.Right);
        ControllerSupport.SubscribeCustomInputs(ProcessEvents);
        ViewModel.PropertyChanged += WhenFilterPropertiesChanged;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ControllerSupport.UnsubscribeCustomInputs(ProcessEvents);
        ViewModel.Dispose();
    }
    
    private async void Last_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToLastPage();
    private async void Next_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToNextPage();

    private void FilterDisplayedPackages(object sender, FilterEventArgs e)
    {
        var item = (IDownloadablePackage)e.Item;
        e.Accepted = true;

        // Filter by ID
        if (ViewModel.HideInstalled && !string.IsNullOrEmpty(item.Id))
        {
            if (_modConfService.ItemsById.ContainsKey(item.Id))
                e.Accepted = false;
        }
    }

    private void WhenFilterPropertiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.HideInstalled))
            _packageViewSource.View.Refresh();
    }

    /// <summary>
    /// Asynchronously loads the image for this control.
    /// </summary>
    private async void OnPreviewImageLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Set our temporary placeholder first
            var image = (Image)sender;
            image.Source = Imaging.GetPlaceholderIcon();

            // Now download the new image async.
            var package = image.DataContext as IDownloadablePackage;
            if (package?.Images == null || package.Images.Length <= 0)
                return;

            // Calculate actual rendered size.
            var tokenSource = new CancellationTokenSource();
            image.Unloaded += (o, args) =>
            {
                image.Loaded -= OnPreviewImageLoaded;
                tokenSource.Cancel();
            };

            var dpiScale = VisualTreeHelper.GetDpi(image);
            var desiredWidth = (int)(image.DesiredSize.Width * dpiScale.DpiScaleX);

            // Select and decode appropriate image.
            var uri = package.Images[0].SelectBasedOnWidth(desiredWidth);
            BitmapImage? result = await Task.Run(async () =>
            {
                await using var memoryStream = new MemoryStream(await _cacheService.GetOrDownloadFileFromUrl(uri, _cacheService.ModPreviewExpiration, false, tokenSource.Token));

                if (!tokenSource.IsCancellationRequested)
                    return Imaging.BitmapFromStream(memoryStream, desiredWidth);

                return null;
            }, tokenSource.Token);
            
            if (!tokenSource.IsCancellationRequested)
                image.Source = result;
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    private void OnClickCard(object sender, MouseButtonEventArgs e)
    {
        // Note: This event is fired before SelectedItem, so we will set the current item ourselves in the meantime.
        HandleCardSelect((IDownloadablePackage)((FrameworkElement)sender).DataContext);
    }

    private void OnPressKeyInListView(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Enter)
        {
            var selectedItem = ((ListView)sender).SelectedItem;
            if (selectedItem != null)
                HandleCardSelect((IDownloadablePackage)selectedItem);

            e.Handled = true;
        }
    }

    private void ProcessEvents(in ControllerState state, ref bool handled)
    {
        if (!state.IsButtonPressed(Button.Accept))
            return;

        if (!WpfUtilities.TryGetFocusedElementAndWindow(out var window, out var element)) 
            return;

        if (element is ListViewItem listViewItem)
        {
            HandleCardSelect((IDownloadablePackage)(listViewItem.DataContext));
            handled = true;
        }
    }

    private void HandleCardSelect(IDownloadablePackage package)
    {
        ViewModel.SelectedResult = package;
        OpenPackagePreviewPage(SlideDirection.Top, SlideDirection.Bottom);
    }

    private void OpenPackagePreviewPage(SlideDirection newPageEnterDirection, SlideDirection oldPageLeaveDirection)
    {
        if (CurrentModPageHost.CurrentPage is PackagePreviewPage oldPage)
        {
            oldPage.SetExitDirection(oldPageLeaveDirection);
            oldPage.Dispose();
        }

        CurrentModPageHost.CurrentPage = new PackagePreviewPage(
            new DownloadPackagePreviewViewModel(ViewModel),
            () => { CurrentModPageHost.CurrentPage = null; },
            newPageEnterDirection);
    }

    private void OptionsPopup_Opened(object sender, EventArgs e) => KeyboardNav.Focus(HideInstalledCheck);

    private void CloseSettingsPopup(object sender, RoutedEventArgs e) => SettingsBtn.IsChecked = false;
}