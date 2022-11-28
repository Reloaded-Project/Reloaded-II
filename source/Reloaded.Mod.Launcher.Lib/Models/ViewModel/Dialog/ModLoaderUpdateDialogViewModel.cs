using Reloaded.Mod.Installer.DependencyInstaller;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
using Environment = System.Environment;
using Version = Reloaded.Mod.Launcher.Lib.Utility.Version;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for the dialog that allows the user to update the Mod Loader.
/// </summary>
public class ModLoaderUpdateDialogViewModel : ObservableObject
{
    /// <summary>
    /// Current progress. Range 0 - 100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// The current Mod Loader verison.
    /// </summary>
    public string CurrentVersion { get; set; }

    /// <summary>
    /// The version the mod loader will be updated to.
    /// </summary>
    public string NewVersion { get; set; }

    /// <summary>
    /// Contains the release changelog.
    /// </summary>
    public string ReleaseText { get; set; } = Resources.UpdateLoaderChangelogUnavailable.Get();

    /// <summary>
    /// URL to the individual release.
    /// </summary>
    public string? ReleaseUrl { get; set; }

    /// <summary>
    /// True if an update can be started;
    /// </summary>
    public bool CanStartUpdate { get; set; } = true;

    private UpdateManager<Empty> _manager;
    private NuGetVersion _targetVersion;

    /// <inheritdoc />
    public ModLoaderUpdateDialogViewModel(UpdateManager<Empty> manager, NuGetVersion targetVersion)
    {
        _manager = manager;
        _targetVersion = targetVersion;

        CurrentVersion = Version.GetReleaseVersion()!.ToNormalizedString();
        NewVersion = _targetVersion.ToString();

        // Try fetch Release info.
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("Reloaded-II"));
            var releases = client.Repository.Release.GetAll(Constants.GitRepositoryAccount, Constants.GitRepositoryName);
            var release = releases.Result.First(x => x.TagName.Contains(targetVersion.ToString()));
            ReleaseUrl = release.HtmlUrl;
            ReleaseText = release.Body;
        }
        catch (Exception) { /* Ignored */ }
    }

    /// <summary>
    /// Performs an update of the mod loader.
    /// </summary>
    public async Task Update()
    {
        CanStartUpdate = false;
        if (ApplicationInstanceTracker.GetAllProcesses(out var processes))
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                // Accumulate list of processes.
                string allProcessString = $"\n{Resources.UpdateLoaderProcessList.Get()}:";
                foreach (var process in processes)
                    allProcessString += $"\n({process.Id}) {process.ProcessName}";

                Actions.DisplayMessagebox.Invoke(Resources.UpdateLoaderRunningTitle.Get(), Resources.UpdateLoaderRunningMessage.Get() + allProcessString, new Actions.DisplayMessageBoxParams()
                {
                    StartupLocation = Actions.WindowStartupLocation.CenterScreen
                });
            });
        }
        else
        {
            _manager.OnApplySelfUpdate += OnApplySelfUpdate;
            await _manager.PrepareUpdateAsync(_targetVersion, new Progress<double>(d => { Progress = d * 100; }));
            await _manager.StartUpdateAsync(_targetVersion, new OutOfProcessOptions() { Restart = true }, new UpdateOptions() { CleanupAfterUpdate = true });
            Environment.Exit(0);
        }
    }

    private void OnApplySelfUpdate(string newUpdateDir)
    {
        var updates = Task.Run(() => DependencyInstaller.GetMissingRuntimeUrls(newUpdateDir)).GetAwaiter().GetResult();
        if (updates.Count <= 0)
            return;

        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
        {
            // Install Updates
            using var tempFolder = new TemporaryFolderAllocation();

            // Display runtime invoke info.
            Actions.DisplayMessagebox.Invoke(Resources.RuntimeUpdateRequiredTitle.Get(), Resources.RuntimeUpdateRequiredDescription.Get(), new Actions.DisplayMessageBoxParams { StartupLocation = Actions.WindowStartupLocation.CenterScreen });

            Task.Run(() => DependencyInstaller.DownloadAndInstallRuntimesAsync(updates, tempFolder.FolderPath, null, null)).Wait();
        });
    }

    /// <summary>
    /// Opens a page that displays the changelog for the current release.
    /// </summary>
    public void ViewChangelog()
    {
        var url = string.IsNullOrEmpty(ReleaseUrl) ? "https://github.com/Reloaded-Project/Reloaded-II/releases" : ReleaseUrl;
        ProcessExtensions.OpenFileWithDefaultProgram(url);
    }
}