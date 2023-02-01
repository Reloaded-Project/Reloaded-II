namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for downloading an individual package.
/// </summary>
public class DownloadPackageViewModel : ObservableObject
{
    private string _modsFolder;

    /// <summary>
    /// Optional text for the dialog.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// List of packages to be downloaded.
    /// </summary>
    public ObservableCollection<IDownloadablePackage> Packages { get; set; } = new ();

    /// <summary>
    /// Token used to control the ongoing package download operation.
    /// </summary>
    public CancellationTokenSource DownloadToken { get; set; } = new CancellationTokenSource();

    /// <summary>
    /// The current progress of the download operation.
    /// Range 0-100
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// The task used for the downloading of the mod.
    /// </summary>
    public Task? DownloadTask { get; set; }

    /// <inheritdoc />
    public DownloadPackageViewModel(IDownloadablePackage package, LoaderConfig config)
    {
        Packages.Add(package);
        _modsFolder = config.GetModConfigDirectory();
    }

    /// <inheritdoc />
    public DownloadPackageViewModel(IEnumerable<IDownloadablePackage> packages, LoaderConfig config)
    {
        Packages.AddRange(packages);
        _modsFolder = config.GetModConfigDirectory();
    }

    /// <summary>
    /// Asynchronously starts the download operation bound by this viewmodel.
    /// </summary>
    public async Task StartDownloadAsync(PackageDownloadState? state = default)
    {
        DownloadToken.Cancel();
        DownloadToken = new CancellationTokenSource();

        state ??= new PackageDownloadState();
        var progressSlicer = new ProgressSlicer(new Progress<double>(d => Progress = d * 100));
        var itemProgress = 1.0 / Packages.Count;

        for (int x = 0; x < Packages.Count; x++)
        {
            var progress = progressSlicer.Slice(itemProgress);
            var package  = Packages[x];
            state.DownloadTasks.Add(package.DownloadAsync(_modsFolder, progress, DownloadToken.Token));
        }

        DownloadTask = Task.WhenAll(state.DownloadTasks);
        await DownloadTask;
    }

    /// <summary>
    /// Encapsulates the state of package downloading operation.
    /// </summary>
    public class PackageDownloadState
    {
        /// <summary>
        /// Contains all of the download tasks.
        /// The strings indicate the folders where the packages were downloaded.
        /// </summary>
        public List<Task<string>> DownloadTasks { get; set; } = new List<Task<string>>();
    }
}