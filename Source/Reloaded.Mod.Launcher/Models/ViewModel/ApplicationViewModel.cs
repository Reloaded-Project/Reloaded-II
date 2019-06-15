using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Commands;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ApplicationViewModel : IDisposable
    {
        public const string LoaderDllName = "Reloaded.Mod.Loader.dll";

        public ImageApplicationPathTuple ApplicationTuple { get; private set; }
        public ManageModsViewModel ManageModsViewModel { get; private set; }
        public ProcessWatcher ProcessWatcher { get; private set; }

        public ObservableCollection<ImageModPathTuple> ModsForThisApp { get; set; }
        public ObservableCollection<Process> ProcessesWithReloaded { get; set; }
        public ObservableCollection<Process> ProcessesWithoutReloaded { get; set; }

        public int ReloadedApps { get; set; }
        public int NonReloadedApps { get; set; }
        public int TotalMods { get; set; }

        private SerialTaskCommand _serialTaskCommand = new SerialTaskCommand();
        private Action<CancellationToken> _onProcessesChanged; 

        public ApplicationViewModel(ImageApplicationPathTuple tuple, ManageModsViewModel modsViewModel)
        {
            ApplicationTuple = tuple;
            ManageModsViewModel = modsViewModel;
            ProcessWatcher = IoC.GetConstant<ProcessWatcher>();
            _onProcessesChanged = ProcessWatcherOnProcessesChanged;

            ManageModsViewModel.ModsChanged += OnModsChanged;
            ProcessWatcher.ProcessesChanged += ProcessWatcherOnProcessesChanged;

            // Update Initial Values
            Task.Run(() =>
            {
                OnModsChanged(null, null);
                ProcessWatcherOnProcessesChanged();
            });
        }

        ~ApplicationViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            ManageModsViewModel.ModsChanged -= OnModsChanged;
            ProcessWatcher.ProcessesChanged -= ProcessWatcherOnProcessesChanged;
            GC.SuppressFinalize(this);
        }

        private void ProcessWatcherOnProcessesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _serialTaskCommand.Execute(_onProcessesChanged);
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

        private void ProcessWatcherOnProcessesChanged(CancellationToken token = default)
        {
            ActionWrappers.TryCatch(() =>
            {
                var processesWithReloaded = new ObservableCollection<Process>();
                var processesWithoutReloaded = new ObservableCollection<Process>();
                string gameExecutableName = ApplicationTuple.ApplicationConfig.GetExecutableName();

                foreach (var process in ProcessWatcher.Processes)
                {
                    if (token.IsCancellationRequested)
                        return;

                    ActionWrappers.TryCatch(() =>
                    {
                        if (process.MainModule.ModuleName == gameExecutableName)
                        {
                            if (IsModLoaderPresent(process))
                                processesWithReloaded.Add(process);
                            else
                                processesWithoutReloaded.Add(process);
                        }
                    });
                }

                ProcessesWithReloaded = processesWithReloaded;
                ProcessesWithoutReloaded = processesWithoutReloaded;
            });
        }

        private bool IsModLoaderPresent(Process process)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == LoaderDllName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
