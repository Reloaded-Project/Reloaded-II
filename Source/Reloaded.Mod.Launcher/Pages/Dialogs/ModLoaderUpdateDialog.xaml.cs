using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Onova;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using ProcessExtensions = Reloaded.Mod.Shared.ProcessExtensions;

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
    }

    public class ModLoaderUpdateDialogViewModel : ObservableObject
    {
        private static XamlResource<string> _xamlUpdateLoaderRunningTitle = new XamlResource<string>("UpdateLoaderRunningTitle");
        private static XamlResource<string> _xamlUpdateLoaderRunningMessage = new XamlResource<string>("UpdateLoaderRunningMessage");
        private static XamlResource<string> _xamlUpdateLoaderProcessList = new XamlResource<string>("UpdateLoaderProcessList");

        public int Progress             { get; set; }
        public string CurrentVersion    { get; set; }
        public string NewVersion        { get; set; }

        private UpdateManager _manager;
        private Version _targetVersion;

        public ModLoaderUpdateDialogViewModel(UpdateManager manager, Version targetVersion)
        {
            _manager = manager; 
            _targetVersion = targetVersion;

            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            NewVersion = _targetVersion.ToString();
        }

        public async Task Update()
        {
            if (ApplicationInstanceTracker.GetAllProcesses(out var processes))
            {
                Application.Current.Dispatcher.Invoke(() =>
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
