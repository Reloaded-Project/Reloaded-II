using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.MVVM;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : ObservableObject, IDisposable
    {
        public const string ModsForThisAppPropertyName = nameof(ModsForThisApp);
        private static readonly object Lock = new object();

        public ImageApplicationPathTuple ApplicationTuple { get; }
        public ManageModsViewModel ManageModsViewModel { get; }
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
        public Task InitializeClassTask { get; }
        private readonly CancellationTokenSource _initializeClassTaskTokenSource;

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

            ManageModsViewModel.ModsChanged -= OnModsChanged;
            if (InstanceTracker != null)
            {
                InstanceTracker.OnProcessesChanged -= InstanceTrackerOnProcessesChanged;
                InstanceTracker.Dispose();
            }

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
