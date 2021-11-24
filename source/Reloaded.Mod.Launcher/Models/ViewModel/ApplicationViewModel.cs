using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Commands.ApplicationPage;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.WPF.Utilities;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : ObservableObject, IDisposable
    {
        private static XamlResource<string> _xamlLoadModSetTitle = new XamlResource<string>("LoadModSetDialogTitle");
        private static XamlResource<string> _xamlSaveModSetTitle = new XamlResource<string>("SaveModSetDialogTitle");
        private static XamlResource<int> _xamlProcessRefreshInterval = new XamlResource<int>("ApplicationHubReloadedProcessRefreshInterval");

        /// <summary>
        /// Executes once list of mods for this app refreshes.
        /// </summary>
        public event Action OnGetModsForThisApp = () => { };

        /// <summary>
        /// Executed when a new Mod Set is loaded by the user.
        /// </summary>
        public event Action OnLoadModSet = () => { };

        public PathTuple<ApplicationConfig> ApplicationTuple { get; }
        public ModConfigService ModConfigService    { get; }
        public ApplicationInstanceTracker InstanceTracker { get; private set; }

        public ObservableCollection<PathTuple<ModConfig>> ModsForThisApp { get; private set; } = new ObservableCollection<PathTuple<ModConfig>>();
        public ObservableCollection<Process> ProcessesWithReloaded    { get; private set; } = new ObservableCollection<Process>();
        public ObservableCollection<Process> ProcessesWithoutReloaded { get; private set; } = new ObservableCollection<Process>();
        public Timer RefreshProcessesWithLoaderTimer { get; private set; }
        public ApplicationSubPage Page { get; private set; }
        public Process SelectedProcess { get; set; }
        public ICommand MakeShortcutCommand { get; set; }

        public ApplicationViewModel(PathTuple<ApplicationConfig> tuple, ModConfigService modConfigService)
        {
            ApplicationTuple    = tuple;
            ModConfigService    = modConfigService;
            MakeShortcutCommand = new MakeShortcutCommand(tuple);

            IoC.Kernel.Rebind<ApplicationViewModel>().ToConstant(this);
            InstanceTracker = new ApplicationInstanceTracker(tuple.Config.AppLocation);
            ModConfigService.Items.CollectionChanged += OnGetModifications;
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
            ModConfigService.Items.CollectionChanged -= OnGetModifications;
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

        /// <summary>
        /// Allows a user to load a new mod set.
        /// </summary>
        public async void LoadModSet()
        {
            var dialog = new VistaOpenFileDialog { Title = _xamlLoadModSetTitle.Get(), Filter = Constants.WpfJsonFormat, AddExtension = true, DefaultExt = ".json" };
            if ((bool)dialog.ShowDialog())
            {
                ModSet.FromFile(dialog.FileName).ToApplicationConfig(ApplicationTuple.Config);
                ApplicationTuple.SaveAsync();
                
                // Check for mod updates/dependencies.
                if (Update.CheckMissingDependencies(out var missingDependencies))
                {
                    try { await Update.DownloadNuGetPackagesAsync(missingDependencies, false, false); }
                    catch (Exception) { }
                }

                CheckModCompatibility();
                OnLoadModSet();
            }
        }

        /// <summary>
        /// Checks if all enabled mods are compatible with this application.
        /// Excludes dependencies.
        /// </summary>
        public void CheckModCompatibility()
        {
            if (Setup.TryGetIncompatibleMods(ApplicationTuple, ModConfigService.Items, out var incompatible))
                new IncompatibleModDialog(incompatible, ApplicationTuple).ShowDialog();
        }

        // == Events ==
        private void RefreshTimerCallback(object state) => UpdateReloadedProcesses();
        private void OnProcessesChanged(Process[] processes) => UpdateReloadedProcesses();
        private void OnGetModifications(object sender, NotifyCollectionChangedEventArgs e) => GetModsForThisApp();

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
            var newMods  = ModConfigService.Items.Where(x => x.Config.SupportedAppId != null && x.Config.SupportedAppId.Contains(appId));

            ModsForThisApp = new ObservableCollection<PathTuple<ModConfig>>(newMods);
            OnGetModsForThisApp();
        }
    }
}
