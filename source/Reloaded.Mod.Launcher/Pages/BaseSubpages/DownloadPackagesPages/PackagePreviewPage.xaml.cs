using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Button = Sewer56.UI.Controller.Core.Enums.Button;
using Image = System.Windows.Controls.Image;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.DownloadPackagesPages;

/// <summary>
/// Interaction logic for PackagePreviewPage.xaml
/// </summary>
public partial class PackagePreviewPage : ReloadedIIPage, IDisposable
{
    public DownloadPackagePreviewViewModel ViewModel { get; set; }

    private SlideDirection _entryDirection;
    private SlideDirection _exitDirection;
    private Action _close;
    private bool _isDisposed;
    private bool _carouselLoaded;

    public PackagePreviewPage(DownloadPackagePreviewViewModel viewModel, Action close, SlideDirection entryDirection)
    {
        SwappedOut += Dispose;
        ViewModel = viewModel;
        _entryDirection = entryDirection;
        _close = close;
        InitializeComponent();
        this.AnimateInFinished -= OnAnimateInFinished;
        PreviewCarousel.OnPageIndexChanged += WhenPageIndexChanged;
        ControllerSupport.SubscribePreviewCustomInputs(SubscribePreviewCustomInputs);
        
        // We need to explicitly set focus in case person changes page on controller without 
        // moving the cursor, otherwise ListViewItem under this page might be re-selected, funny issue.
        this.AnimateInStarted += OnAnimateInStarted;
    }

    private void OnAnimateInStarted()
    {
        this.AnimateInStarted -= OnAnimateInStarted;
        var focusVisualSetting = KeyboardNav.AlwaysShowFocusVisual;
        try
        {
            KeyboardNav.AlwaysShowFocusVisual = false;
            Keyboard.Focus(CloseBtn);
        }
        finally
        {
            KeyboardNav.AlwaysShowFocusVisual = focusVisualSetting;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        ControllerSupport.UnsubscribePreviewCustomInputs(SubscribePreviewCustomInputs);
        PreviewCarousel?.Dispose();
    }

    /// <summary>
    /// Sets the direction of travel of the page upon exit.
    /// </summary>
    /// <param name="direction">The direction of travel.</param>
    public void SetExitDirection(SlideDirection direction) => _exitDirection = direction;

    /// <summary>
    /// Add all images to carousel on load.
    /// </summary>
    private async void OnCarouselLoaded(object sender, RoutedEventArgs e)
    {
        if (_isDisposed || _carouselLoaded)
            return;

        _carouselLoaded = true;
        var package = ViewModel.Package;
        var carousel = (Carousel)sender;
        if (package.Images is not { Length: > 0 })
        {
            AddImageFromBitmap(carousel, Imaging.GetPlaceholderIcon());
            return;
        }

        var images = package.Images;
        for (int x = 0; x < images.Length; x++)
            await AddImageFromUri(carousel, images[x].Uri);
    }

    /// <summary>
    /// Downloads an image from a given URI and adds it to the carousel
    /// </summary>
    /// <param name="carousel">The carousel to add image to.</param>
    /// <param name="uri">Uri to download images from.</param>
    private async Task AddImageFromUri(Carousel carousel, Uri uri)
    {
        // We're using ConfigureAwait(false) to allow other threads to pick this up.
        // Helps prevent potential stutters when converting image.
        var data = await ViewModel.DownloadImage(uri).ConfigureAwait(false);
        await using var memoryStream = new MemoryStream(data);
        var image = Imaging.BitmapFromStream(memoryStream); // Costly!
        AddImageFromBitmap(carousel, image);
    }

    /// <summary>
    /// Adds an image to the carousel based on bitmap.
    /// </summary>
    /// <param name="carousel">The carousel to add image to.</param>
    /// <param name="bitmap">The image to apply to the bitmap.</param>
    private void AddImageFromBitmap(Carousel carousel, BitmapImage bitmap)
    {
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            var image = new Image();
            image.Source = bitmap;
            image.Height = carousel.Height;
            image.Width = carousel.Width;
            carousel.Items.Add(image);
            carousel.UpdatePageButtons();
        });
    }

    /// <summary>
    /// Creates the page entry animation.
    /// </summary>
    protected override Animation[] MakeEntryAnimations()
    {
        var opacityAnimation = new OpacityAnimation(XamlEntryFadeAnimationDuration.Get(), XamlEntryFadeOpacityStart.Get(), 1.0);
        switch (_entryDirection)
        {
            case SlideDirection.Top:
                return new Animation[]
                {
                    new RenderTransformAnimation(-ActualHeight, RenderTransformDirection.Vertical,
                        RenderTransformTarget.Towards, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };

            case SlideDirection.Bottom:
                return new Animation[]
                {
                    new RenderTransformAnimation(ActualHeight, RenderTransformDirection.Vertical,
                        RenderTransformTarget.Towards, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };

            case SlideDirection.Left:
            default:
                return new Animation[]
                {
                    new RenderTransformAnimation(-ActualWidth, RenderTransformDirection.Horizontal,
                        RenderTransformTarget.Towards, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };

            case SlideDirection.Right:
                return new Animation[]
                {
                    new RenderTransformAnimation(ActualWidth, RenderTransformDirection.Horizontal,
                        RenderTransformTarget.Towards, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };
        }
    }

    /// <summary>
    /// Creates the page exit animation.
    /// </summary>
    protected override Animation[] MakeExitAnimations()
    {
        var opacityAnimation = new OpacityAnimation(XamlExitFadeAnimationDuration.Get(), 1.0, XamlExitFadeOpacityEnd.Get());
        switch (_exitDirection)
        {
            case SlideDirection.Top:
                return new Animation[]
                {
                    new RenderTransformAnimation(-ActualHeight, RenderTransformDirection.Vertical,
                        RenderTransformTarget.Away, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };

            case SlideDirection.Bottom:
                return new Animation[]
                {
                    new RenderTransformAnimation(ActualHeight, RenderTransformDirection.Vertical,
                        RenderTransformTarget.Away, duration: XamlEntrySlideAnimationDuration.Get()),
                    opacityAnimation
                };


            case SlideDirection.Left:
            default:
                return new Animation[]
                {
                    new RenderTransformAnimation(-ActualWidth, RenderTransformDirection.Horizontal,
                        RenderTransformTarget.Away, duration: XamlExitSlideAnimationDuration.Get()),
                    opacityAnimation
                };

            case SlideDirection.Right:
                return new Animation[]
                {
                    new RenderTransformAnimation(ActualWidth, RenderTransformDirection.Horizontal,
                        RenderTransformTarget.Away, duration: XamlExitSlideAnimationDuration.Get()),
                    opacityAnimation
                };
        }
    }
    
    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram(e.Parameter.ToString()!);
    
    // Don't navigate hyperlinks in our markdown, thanks!
    private void Page_Click(object sender, RoutedEventArgs e) => e.Handled = true;

    private void WhenPageIndexChanged(int pageIndex)
    {
        if (ViewModel.Package.Images != null)
            ViewModel.SelectedImage = ViewModel.Package.Images[pageIndex];
    }

    private void Click_Close(object sender, RoutedEventArgs e)
    {
        _exitDirection = SlideDirection.Bottom;
        Close();
    }

    private void Click_OpenProjectUrl(object sender, RoutedEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram(ViewModel.Package.ProjectUri!.ToString());

    // Add window close support for controller, and page switch.
    private void SubscribePreviewCustomInputs(in ControllerState state, ref bool handled)
    {
        if (state.IsButtonPressed(Button.Decline))
        {
            handled = true;
            Close();
            return;
        }

        if (state.IsButtonPressed(Button.LastPage))
        {
            handled = true;
            if (ViewModel.SelectLastItem.CanExecute(null))
                ViewModel.SelectLastItem.Execute(null);

            return;
        }

        if (state.IsButtonPressed(Button.NextPage))
        {
            handled = true;
            if (ViewModel.SelectNextItem.CanExecute(null))
                ViewModel.SelectNextItem.Execute(null);

            return;
        }

        if (state.IsButtonPressed(Button.Decrement))
        {
            handled = true;
            PreviewCarousel.PageIndex -= 1;
            return;
        }

        if (state.IsButtonPressed(Button.Increment))
        {
            handled = true;
            PreviewCarousel.PageIndex += 1;
            return;
        }
    }

    private void Close()
    {
        Dispose();
        _close();
    }

    // Show appropriate source image.
    private void OnSourceImageLoaded(object sender, RoutedEventArgs e)
    {
        var source = ViewModel.Package.Source;
        var image = (Image)sender;
        
        switch (source)
        {
            case GameBananaPackageProvider.SourceName:
                image.Source = (ImageSource)FindResource("IconSourceGameBanana");
                break;

            default:
                image.Source = Imaging.BitmapFromUri(WpfConstants.PlaceholderImagePath);
                break;
        }
    }
}

/// <summary/>
public enum SlideDirection
{
    Top,
    Bottom,
    Left,
    Right
}