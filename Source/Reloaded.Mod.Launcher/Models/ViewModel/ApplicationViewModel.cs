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
        public const string ModsForThisAppPropertyName = nameof(ModsForThisApp);
        private static XamlResource<int> _xamlProcessRefreshInterval = new XamlResource<int>("ApplicationHubReloadedProcessRefreshInterval");

        private static readonly object Lock = new object();
        private static Process[] _emptyProcessArray = new Process[0];

        public ImageApplicationPathTuple ApplicationTuple { get; }
        public ManageModsViewModel ManageModsViewModel { get; }
        public ApplicationInstanceTracker InstanceTracker { get; private set; }

        public ObservableCollection<ImageModPathTuple> ModsForThisApp { get; private set; }
        public ObservableCollection<Process> ProcessesWithReloaded { get; private set; }
        public ObservableCollection<Process> ProcessesWithoutReloaded { get; private set; }
        public Timer RefreshProcessesWithLoaderTimer { get; private set; }

        public int ReloadedApps { get; set; }
        public int NonReloadedApps { get; set; }
        public int TotalMods { get; set; }

        public ApplicationSubPage Page
        {
            get => _page;
            set => _page = value;
        }

        public Process SelectedProcess { get; set; }

        /// <summary>
        /// The task that handles asynchronous initialization of this viewmodel.
        /// Child pages should check that it completed before initializing self.
        /// </summary>
        public Task InitializeClassTask { get; }
        private readonly CancellationTokenSource _initializeClassTaskTokenSource;
        private ApplicationSubPage _page;

        public ApplicationViewModel(ImageApplicationPathTuple tuple, ManageModsViewModel modsViewModel)
        {
            ApplicationTuple = tuple;
            ManageModsViewModel = modsViewModel;

            // Update Initial Values
            _initializeClassTaskTokenSource = new CancellationTokenSource();
            lock (Lock)
            {
                // Rebind needs to be atomic or otherwise when default page's viewmodel (ApplicationSummaryViewModel)
                // picks up this viewmodel, there may be multiple bindings.
                IoC.Kernel.Rebind<ApplicationViewModel>().ToConstant(this);
            }

            InitializeClassTask = Task.Run(() =>
            {
                try
                {
                    InstanceTracker = new ApplicationInstanceTracker(tuple.ApplicationConfig.AppLocation, _initializeClassTaskTokenSource.Token);
                    ManageModsViewModel.ModsChanged += OnModsChanged;
                    InstanceTracker.OnProcessesChanged += InstanceTrackerOnProcessesChanged;

                    InstanceTrackerOnProcessesChanged(new Process[0]);
                    OnModsChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    RefreshProcessesWithLoaderTimer = new Timer(state => { InstanceTrackerOnProcessesChanged(_emptyProcessArray); }, null, 500, _xamlProcessRefreshInterval.Get());
                    Page = ApplicationSubPage.ApplicationSummary;
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var box = new MessageBox(Errors.Error(), $"{ex.Message} | {ex.StackTrace}");
                        box.ShowDialog();
                    });
                }
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

            RefreshProcessesWithLoaderTimer?.Dispose();
            ManageModsViewModel.ModsChanged -= OnModsChanged;
            if (InstanceTracker != null)
            {
                InstanceTracker.OnProcessesChanged -= InstanceTrackerOnProcessesChanged;
                InstanceTracker.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void ChangePageProperty(ApplicationSubPage page)
        {
            _page = page;
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
