using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NuGet.Versioning;
using Octokit;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using Sewer56.Update;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Structures;
using Constants = Reloaded.Mod.Launcher.Misc.Constants;
using Version = Reloaded.Mod.Launcher.Utility.Version;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstLaunch.xaml
    /// </summary>
    public partial class ModLoaderUpdateDialog : ReloadedWindow
    {
        public new ModLoaderUpdateDialogViewModel ViewModel { get; set; }

        public ModLoaderUpdateDialog(UpdateManager<Empty> manager, NuGetVersion targetVersion)
        {
            InitializeComponent();
            ViewModel = new ModLoaderUpdateDialogViewModel(manager, targetVersion);
            var model = (WindowViewModel) this.DataContext;
            model.MinimizeButtonVisibility = Visibility.Collapsed;
            model.MaximizeButtonVisibility = Visibility.Collapsed;
        }

        private void ViewChangelogClick(object sender, RoutedEventArgs e)
        {
            var url = string.IsNullOrEmpty(ViewModel.ReleaseUrl) ? "https://github.com/Reloaded-Project/Reloaded-II/releases" : ViewModel.ReleaseUrl;
            ProcessExtensions.OpenFileWithDefaultProgram(url);
        }

        private async void UpdateClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.Update();
        }
        
        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram(e.Parameter.ToString());
    }

    public class ModLoaderUpdateDialogViewModel : ObservableObject
    {
        private static XamlResource<string> _xamlUpdateLoaderRunningTitle = new XamlResource<string>("UpdateLoaderRunningTitle");
        private static XamlResource<string> _xamlUpdateLoaderRunningMessage = new XamlResource<string>("UpdateLoaderRunningMessage");
        private static XamlResource<string> _xamlUpdateLoaderProcessList = new XamlResource<string>("UpdateLoaderProcessList");
        private static XamlResource<string> _xamlUpdateLoaderChangelogUnavailable = new XamlResource<string>("UpdateLoaderChangelogUnavailable");

        public int Progress             { get; set; }
        public string CurrentVersion    { get; set; }
        public string NewVersion        { get; set; }
        public string ReleaseText       { get; set; } = _xamlUpdateLoaderChangelogUnavailable.Get();
        public string ReleaseUrl        { get; set; }

        private UpdateManager<Empty> _manager;
        private NuGetVersion _targetVersion;

        public ModLoaderUpdateDialogViewModel(UpdateManager<Empty> manager, NuGetVersion targetVersion)
        {
            _manager = manager; 
            _targetVersion = targetVersion;

            CurrentVersion = Version.GetReleaseVersion().ToNormalizedString();
            NewVersion     = _targetVersion.ToString();

            // Try fetch Release info.
            try
            {
                var client      = new GitHubClient(new ProductHeaderValue("Reloaded-II"));
                var releases    = client.Repository.Release.GetAll(Misc.Constants.GitRepositoryAccount, Constants.GitRepositoryName);
                var release     = releases.Result.First(x => x.TagName.Contains(targetVersion.ToString()));
                ReleaseUrl      = release.HtmlUrl;
                ReleaseText     = release.Body;
            }
            catch (Exception) { /* Ignored */ }
        }

        public async Task Update()
        {
            if (ApplicationInstanceTracker.GetAllProcesses(out var processes))
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                {
                    // Accumulate list of processes.
                    string allProcessString = $"\n{_xamlUpdateLoaderProcessList.Get()}:";
                    foreach (var process in processes)
                        allProcessString += $"\n({process.Id}) {process.ProcessName}";

                    var box = new MessageBox(_xamlUpdateLoaderRunningTitle.Get(), _xamlUpdateLoaderRunningMessage.Get() + allProcessString);
                    box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    box.ShowDialog();
                });
            }
            else
            {
                await _manager.PrepareUpdateAsync(_targetVersion, new Progress<double>(d => { Progress = (int)(d * 100); }));
                await _manager.StartUpdateAsync(_targetVersion, new OutOfProcessOptions() { Restart = true }, null);
                Environment.Exit(0);
            }
        }
    }
}
