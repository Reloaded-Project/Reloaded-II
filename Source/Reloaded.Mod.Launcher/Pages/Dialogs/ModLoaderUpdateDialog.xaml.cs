using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using Onova;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Shared;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using Constants = Reloaded.Mod.Launcher.Misc.Constants;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstLaunch.xaml
    /// </summary>
    public partial class ModLoaderUpdateDialog : ReloadedWindow
    {
        public new ModLoaderUpdateDialogViewModel ViewModel { get; set; }

        public ModLoaderUpdateDialog(UpdateManager manager, Version targetVersion)
        {
            InitializeComponent();
            ViewModel = new ModLoaderUpdateDialogViewModel(manager, targetVersion);
            var model = (WindowViewModel) this.DataContext;
            model.MinimizeButtonVisibility = Visibility.Collapsed;
            model.MaximizeButtonVisibility = Visibility.Collapsed;
        }

        private void ViewChangelogClick(object sender, RoutedEventArgs e)
        {
            ProcessExtensions.OpenFileWithDefaultProgram("https://github.com/Reloaded-Project/Reloaded-II/releases");
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

        private UpdateManager _manager;
        private Version _targetVersion;

        public ModLoaderUpdateDialogViewModel(UpdateManager manager, Version targetVersion)
        {
            _manager = manager; 
            _targetVersion = targetVersion;

            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            NewVersion     = _targetVersion.ToString();

            // Try fetch Release info.
            try
            {
                var client      = new GitHubClient(new ProductHeaderValue("Reloaded-II"));
                var releases    = client.Repository.Release.GetAll(Misc.Constants.GitRepositoryAccount, Constants.GitRepositoryName);
                var release     = releases.Result.First(x => x.TagName.Contains(targetVersion.ToString()));
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
                _manager.LaunchUpdater(_targetVersion, true);
                Environment.Exit(0);
            }
        }
    }
}
