using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Commands.DownloadModsPage;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Utilities;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : ObservableObject, IDisposable
    {
        private static XamlResource<string> _xamlLoadModSetTitle = new XamlResource<string>("LoadModSetDialogTitle");
        private static XamlResource<string> _xamlSaveModSetTitle = new XamlResource<string>("SaveModSetDialogTitle");
        private static XamlResource<int> _xamlProcessRefreshInterval = new XamlResource<int>("ApplicationHubReloadedProcessRefreshInterval");
        private static CheckForUpdatesAndDependenciesCommand _checkForUpdatesAndDependenciesCommand = new CheckForUpdatesAndDependenciesCommand();

        /// <summary>
        /// Executes once the user loads a new mod set.
        /// </summary>
        public event Action OnLoadModSet = () => { };

        public ImageApplicationPathTuple ApplicationTuple { get; }
        public ManageModsViewModel ManageModsViewModel    { get; }
        public ApplicationInstanceTracker InstanceTracker { get; private set; }

        public ObservableCollection<ImageModPathTuple> ModsForThisApp { get; private set; }
        public ObservableCollection<Process> ProcessesWithReloaded { get; private set; } = new ObservableCollection<Process>();
        public ObservableCollection<Process> ProcessesWithoutReloaded { get; private set; } = new ObservableCollection<Process>();
        public Timer RefreshProcessesWithLoaderTimer { get; private set; }
        public ApplicationSubPage Page { get; private set; }
        public Process SelectedProcess { get; set; }

        public ApplicationViewModel(ImageApplicationPathTuple tuple, ManageModsViewModel modsViewModel)
        {
            ApplicationTuple    = tuple;
            ManageModsViewModel = modsViewModel;

            IoC.Kernel.Rebind<ApplicationViewModel>().ToConstant(this);
            InstanceTracker = new ApplicationInstanceTracker(tuple.Config.AppLocation);
            ManageModsViewModel.ModsChanged += OnModsChanged;
            ManageModsViewModel.ModSaving += OnModSaving;
            InstanceTracker.OnProcessesChanged += OnProcessesChanged;

            UpdateReloadedProcesses();
            GetModsForThisApp();
            RefreshProcessesWithLoaderTimer = new Timer(RefreshTimerCallback, null, 500, _xamlProcessRefreshInterval.Get());
            Page = ApplicationSubPage.ApplicationSummary;
        }

        ~ApplicationViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            ManageModsViewModel.ModsChanged -= OnModsChanged;
            ManageModsViewModel.ModSaving -= OnModSaving;
            InstanceTracker.OnProcessesChanged -= OnProcessesChanged;

            RefreshProcessesWithLoaderTimer?.Dispose();
            InstanceTracker?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Changes the currently opened page in the application submenu.
        /// </summary>
        public void ChangeApplicationPage(ApplicationSubPage page) => Page = page;

        /// <summary>
        /// Allows a user to load a new mod set.
        /// </summary>
        public async void LoadModSet()
        {
            var dialog = new VistaOpenFileDialog { Title = _xamlLoadModSetTitle.Get(), Filter = Constants.WpfJsonFormat, AddExtension = true, DefaultExt = ".json" };
            if ((bool)dialog.ShowDialog())
            {
                ModSet.FromFile(dialog.FileName).ToApplicationConfig(ApplicationTuple.Config);
                ApplicationTuple.Save();
                OnLoadModSet();

                // Check for mod updates/dependencies.
                await Task.Run(Update.CheckForModUpdatesAsync);
                if (Update.CheckMissingDependencies(out var missingDependencies))
                {
                    try { await Update.DownloadPackagesAsync(missingDependencies, false, false); }
                    catch (Exception e) { }
                }
            }
        }

        /// <summary>
        /// Allows the user to save a mod set.
        /// </summary>
        public void SaveModSet()
        {
            var dialog = new VistaSaveFileDialog { Title = _xamlSaveModSetTitle.Get(), Filter = Constants.WpfJsonFormat, AddExtension = true, DefaultExt = ".json" };
            if ((bool)dialog.ShowDialog())
            {
                new ModSet(ApplicationTuple.Config).Save(dialog.FileName);
            }
        }

        // == Events ==
        private void RefreshTimerCallback(object state) => UpdateReloadedProcesses();
        private void OnProcessesChanged(Process[] processes) => UpdateReloadedProcesses();
        private void OnModSaving(ImageModPathTuple pathTuple) => GetModsForThisApp();
        private void OnModsChanged(object sender, NotifyCollectionChangedEventArgs args) => GetModsForThisApp();

        private void UpdateReloadedProcesses()
        {
            var result = InstanceTracker.GetProcesses();
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                Collections.ModifyObservableCollection(ProcessesWithReloaded, result.ReloadedProcesses);
                Collections.ModifyObservableCollection(ProcessesWithoutReloaded, result.NonReloadedProcesses);
            });
        }

        private void GetModsForThisApp()
        {
            string appId = ApplicationTuple.Config.AppId;
            var newMods  = ManageModsViewModel.Mods.Where(x => x.ModConfig.SupportedAppId != null && x.ModConfig.SupportedAppId.Contains(appId));

            ModsForThisApp = new ObservableCollection<ImageModPathTuple>(newMods);
        }
    }
}
