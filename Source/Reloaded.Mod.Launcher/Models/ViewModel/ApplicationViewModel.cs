using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.MVVM;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : ObservableObject, IDisposable
    {
        public const string ModsForThisAppPropertyName = nameof(ModsForThisApp);
        private static object _lock = new object();

        public ImageApplicationPathTuple ApplicationTuple { get; private set; }
        public ManageModsViewModel ManageModsViewModel { get; private set; }
        public ApplicationInstanceTracker InstanceTracker { get; private set; }

        public ObservableCollection<ImageModPathTuple> ModsForThisApp { get; set; }
        public ObservableCollection<Process> ProcessesWithReloaded { get; set; }
        public ObservableCollection<Process> ProcessesWithoutReloaded { get; set; }

        public int ReloadedApps { get; set; }
        public int NonReloadedApps { get; set; }
        public int TotalMods { get; set; }

        public ApplicationSubPage Page { get; set; }
        public Process SelectedProcess { get; set; }

        /// <summary>
        /// The task that handles asynchronous initialization of this viewmodel.
        /// Child pages should check that it completed before initializing self.
        /// </summary>
        public Task InitializeClassTask { get; private set; }
        private CancellationTokenSource _initializeClassTaskTokenSource;

        public ApplicationViewModel(ImageApplicationPathTuple tuple, ManageModsViewModel modsViewModel)
        {
            ApplicationTuple = tuple;
            ManageModsViewModel = modsViewModel;

            // Update Initial Values
            _initializeClassTaskTokenSource = new CancellationTokenSource();
            lock (_lock)
            {
                // Rebind needs to be atomic or otherwise when default page's viewmodel (ApplicationSummaryViewModel)
                // picks up this viewmodel, there may be multiple bindings.
                IoC.Kernel.Rebind<ApplicationViewModel>().ToConstant(this);
            }

            InitializeClassTask = Task.Run(() =>
            {
                InstanceTracker = new ApplicationInstanceTracker(tuple.ApplicationConfig.AppLocation, _initializeClassTaskTokenSource.Token);
                ManageModsViewModel.ModsChanged += OnModsChanged;
                InstanceTracker.OnProcessesChanged += InstanceTrackerOnProcessesChanged;

                InstanceTrackerOnProcessesChanged(new Process[0]);
                OnModsChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                Page = ApplicationSubPage.ApplicationSummary;
            });
        }

        ~ApplicationViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            // Safeguard for those spamming the application button.
            if (!InitializeClassTask.IsCompleted)
            {
                _initializeClassTaskTokenSource.Cancel();
                InitializeClassTask.Wait();
            }

            ManageModsViewModel.ModsChanged -= OnModsChanged;
            InstanceTracker.OnProcessesChanged -= InstanceTrackerOnProcessesChanged;
            GC.SuppressFinalize(this);
        }

        public void RaisePagePropertyChanged()
        {
            RaisePropertyChangedEvent(nameof(Page));
        }

        private void InstanceTrackerOnProcessesChanged(Process[] newProcesses)
        {
            var result = InstanceTracker.GetProcesses();
            ProcessesWithReloaded = new ObservableCollection<Process>(result.ReloadedProcesses);
            ProcessesWithoutReloaded = new ObservableCollection<Process>(result.NonReloadedProcesses);
            ReloadedApps = ProcessesWithReloaded.Count;
            NonReloadedApps = ProcessesWithoutReloaded.Count;
        }

        private void OnModsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Calculate new mod set.
            var newMods = new ObservableCollection<ImageModPathTuple>();
            string appId = ApplicationTuple.ApplicationConfig.AppId;
            foreach (var mod in ManageModsViewModel.Mods)
            {
                if (mod.ModConfig.SupportedAppId.Contains(appId))
                    newMods.Add(mod);
            }

            // Set count.
            ModsForThisApp = newMods;
            TotalMods = newMods.Count;
        }
    }
}
