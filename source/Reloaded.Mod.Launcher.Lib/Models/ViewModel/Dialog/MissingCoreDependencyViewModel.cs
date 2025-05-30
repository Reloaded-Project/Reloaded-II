using Reloaded.Mod.Installer.DependencyInstaller;
using Reloaded.Mod.Installer.DependencyInstaller.IO;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for displaying a list of missing .NET Core Dependencies.
/// </summary>
public class MissingCoreDependencyDialogViewModel : ObservableObject
{
    /// <summary>
    /// List of missing dependencies to run Reloaded.
    /// </summary>
    public required ObservableCollection<MissingDependency> Dependencies { get; init; }

    /// <summary>
    /// Progress of the current download operation.
    /// Range 0 - 100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// True if user can press download button, else false.
    /// </summary>
    public bool CanDownload { get; set; } = true;

    /// <summary>
    /// The current step being performed during the download/installation process.
    /// </summary>
    public string CurrentStep { get; set; } = "";

    /// <summary>
    /// Checks current dependencies, returning a new instance of this ViewModel if any dependencies are missing.
    /// </summary>
    /// <param name="launcherLocation"></param>
    public static async Task<MissingCoreDependencyDialogViewModel> CreateAsync(string launcherLocation)
    {
        var deps = await DependencyInstaller.GetMissingDependencies(launcherLocation);
        return new MissingCoreDependencyDialogViewModel()
        {
            Dependencies = new ObservableCollection<MissingDependency>(deps)
        };
    }

    /// <summary>
    /// True if any dependencies are missing.
    /// </summary>
    public bool MissingDependencies => Dependencies.Count > 0;

    /// <summary>
    /// Downloads and installs all missing dependencies with progress reporting.
    /// </summary>
    public async Task<bool> DownloadAndInstallMissingDependenciesAsync()
    {
        CanDownload = false;
        try
        {
            using var tempDownloadDir = new TemporaryFolderAllocation();
            await DependencyInstaller.DownloadAndInstallRuntimesAsync(Dependencies, tempDownloadDir.FolderPath, 
                new Progress<double>(d => Progress = d * 100), 
                step => CurrentStep = step);
            return true;
        }
        finally
        {
            CanDownload = true;
            CurrentStep = "";
        }
    }
}