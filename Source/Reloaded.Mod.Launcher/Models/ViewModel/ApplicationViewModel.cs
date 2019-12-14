using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Utilities;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : ObservableObject, IDisposable
    {
        private static XamlResource<int> _xamlProcessRefreshInterval = new XamlResource<int>("ApplicationHubReloadedProcessRefreshInterval");

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
