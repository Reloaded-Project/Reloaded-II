namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.DownloadPackages;

/// <summary>
/// Viewmodel for previewing of a downloaded package.
/// </summary>
public class DownloadPackagePreviewViewModel : ObservableObject
{
    /// <summary>
    /// The package to create the viewmodel from.
    /// </summary>
    public IDownloadablePackage Package { get; set; }

    /// <summary>
    /// Status of the current package download.
    /// </summary>
    public DownloadPackageStatus DownloadPackageStatus { get; set; }

    /// <summary>
    /// Command used to download an individual mod.
    /// </summary>
    public DownloadPackageCommand DownloadModCommand { get; set; }

    /// <summary>
    /// The currently displaying image.
    /// </summary>
    public DownloadableImage SelectedImage { get; set; }

    /// <summary>
    /// Selects the next package for viewing.
    /// </summary>
    public RelayCommand SelectNextItem { get; set; }

    /// <summary>
    /// Selects the previous package for viewing.
    /// </summary>
    public RelayCommand SelectLastItem { get; set; }

    /// <summary>
    /// Provides access to the image caching service.
    /// </summary>
    public ImageCacheService CacheService { get; }

    /// <summary>
    /// Creates a viewmodel used to download a package.
    /// </summary>
    /// <param name="parent">The parent to create this viewmodel from. Copies currently selected package.</param>
    public DownloadPackagePreviewViewModel(DownloadPackagesViewModel parent)
    {
        Package = parent.SelectedResult;
        DownloadPackageStatus = parent.DownloadPackageStatus;
        DownloadModCommand = parent.DownloadModCommand;
        SelectNextItem = parent.SelectNextItem;
        SelectLastItem = parent.SelectLastItem;
        CacheService = IoC.GetConstant<ImageCacheService>();

        // Select default image
        if (Package.Images is { Length: > 0 })
            SelectedImage = Package.Images[0];
    }
}