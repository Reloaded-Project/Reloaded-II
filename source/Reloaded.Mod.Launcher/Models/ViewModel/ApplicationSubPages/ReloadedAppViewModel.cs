using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Dialogs;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.WPF.Utilities;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ReloadedAppViewModel : ObservableObject, IDisposable
    {
        private static XamlResource<int> _xamlModLoaderSetupTimeout = new XamlResource<int>("ReloadedProcessModLoaderSetupTimeout");
        private static XamlResource<int> _xamlModLoaderSetupSleepTime = new XamlResource<int>("ReloadedProcessModLoaderSetupSleepTime");
        private static XamlResource<int> _xamlModLoaderRefreshInterval = new XamlResource<int>("ReloadedProcessModListRefreshInterval");


        public ApplicationViewModel          ApplicationViewModel   { get; set; }
        public Client                        Client                 { get; set; }
        public ModInfo                       SelectedMod            { get; set; }
        public ObservableCollection<ModInfo> CurrentMods            { get; set; } = new ObservableCollection<ModInfo>();

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private System.Timers.Timer _refreshTimer;

        public ReloadedAppViewModel(ApplicationViewModel applicationViewModel)
        {
            ApplicationViewModel = applicationViewModel;
            ApplicationViewModel.SelectedProcess.EnableRaisingEvents = true;
            ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;

            /* Try establish connection. */
            int port = 0;
            try
            {
                port = ActionWrappers.TryGetValue(GetPort, _xamlModLoaderSetupTimeout.Get(), _xamlModLoaderSetupSleepTime.Get());
            }
            catch (Exception ex)
            {
                Errors.HandleException(new Exception(Errors.ErrorFailedToObtainPort(), ex));
                return;
            }

            Client = new Client(port);
            Client.OnReceiveException += ClientOnReceiveException;
            Refresh();

            _refreshTimer = new System.Timers.Timer(_xamlModLoaderRefreshInterval.Get());
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += (sender, args) => Refresh();
            _refreshTimer.Enabled = true;
        }

        ~ReloadedAppViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            Client.OnReceiveException -= ClientOnReceiveException;
            _cancellationTokenSource?.Cancel();
            _refreshTimer?.Dispose();
        }

        private void SelectedProcessOnExited(object sender, EventArgs e)
        {
            ApplicationViewModel.SelectedProcess.Exited -= SelectedProcessOnExited;
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
            ActionWrappers.TryCatchDiscard(() => SelectedMod = CurrentMods[0]);
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
                    Collections.ModifyObservableCollection(CurrentMods, loadedMods.Mods);
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
    }
}
