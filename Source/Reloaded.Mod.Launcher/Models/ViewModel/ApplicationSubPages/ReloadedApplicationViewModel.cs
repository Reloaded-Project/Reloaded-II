using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Dialogs;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ReloadedApplicationViewModel : ObservableObject, IDisposable
    {
        private static XamlResource<int> _xamlModLoaderSetupTimeout = new XamlResource<int>("ReloadedProcessModLoaderSetupTimeout");
        private static XamlResource<int> _xamlModLoaderSetupSleepTime = new XamlResource<int>("ReloadedProcessModLoaderSetupSleepTime");
        private static XamlResource<int> _xamlModLoaderRefreshInterval = new XamlResource<int>("ReloadedProcessModListRefreshInterval");


        public ApplicationViewModel          ApplicationViewModel   { get; set; }
        public Client                        Client                 { get; set; }
        public ModInfo                       SelectedMod            { get; set; }
        public ObservableCollection<ModInfo> CurrentMods
        {
            get => _currentMods;
            set => _currentMods = value;
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private System.Timers.Timer _refreshTimer;
        private ObservableCollection<ModInfo> _currentMods;

        public ReloadedApplicationViewModel(ApplicationViewModel applicationViewModel)
        {
            ApplicationViewModel = applicationViewModel;
            ApplicationViewModel.SelectedProcess.EnableRaisingEvents = true;
            ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;

            /* Try establish connection. */
            int port = ActionWrappers.TryGetValue(GetPort, _xamlModLoaderSetupTimeout.Get(), _xamlModLoaderSetupSleepTime.Get());

            Client = new Client(port);
            Client.OnReceiveException += ClientOnReceiveException;
            Refresh();

            _refreshTimer = new System.Timers.Timer(_xamlModLoaderRefreshInterval.Get());
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += (sender, args) => Refresh();
            _refreshTimer.Enabled = true;
        }

        private void SelectedProcessOnExited(object sender, EventArgs e)
        {
            ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
        }

        private void ClientOnReceiveException(GenericExceptionResponse obj)
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                var box = new Pages.Dialogs.MessageBox(Errors.Error(), obj.Message);
                box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                box.ShowDialog();
            });
        }

        /// <summary>
        /// Resets the selected index.
        /// </summary>
        public void ResetSelectedMod()
        {
            ActionWrappers.TryCatch(() => SelectedMod = CurrentMods[0]);
        }

        /* Actions */
        public void Unload()    => Task.Run(UnloadTask).ContinueWith((ack) => Refresh());
        public void Suspend()   => Task.Run(SuspendTask).ContinueWith((ack) => Refresh());
        public void Resume()    => Task.Run(ResumeTask).ContinueWith((ack) => Refresh());

        Task<Acknowledgement>       UnloadTask()   => Client?.UnloadModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
        Task<Acknowledgement>       SuspendTask()  => Client?.SuspendModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
        Task<Acknowledgement>       ResumeTask()   => Client?.ResumeModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
        Task<GetLoadedModsResponse> RefreshTask()  => Client?.GetLoadedModsAsync(1000, _cancellationTokenSource.Token);

        public async void Refresh()
        {
            try
            {
                var loadedMods = await Task.Run(RefreshTask);
                ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                {
                    Collections.UpdateObservableCollection(ref _currentMods, loadedMods.Mods);
                    RaisePropertyChangedEvent(nameof(CurrentMods));
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Acquires the port to connect to the remote process.
        /// </summary>
        private int GetPort()
        {
            int pid             = ApplicationViewModel.SelectedProcess.Id;
            return Client.GetPort(pid);
        }

        public void ShowLoadModDialog()
        {
            var loadModSelectDialog     = new LoadModSelectDialog(ApplicationViewModel, this);
            loadModSelectDialog.Owner   = Application.Current.MainWindow;
            loadModSelectDialog.ShowDialog();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _refreshTimer?.Dispose();
        }
    }
}
