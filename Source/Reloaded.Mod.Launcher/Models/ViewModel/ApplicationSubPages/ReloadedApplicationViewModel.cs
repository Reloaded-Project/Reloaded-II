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

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ReloadedApplicationViewModel : ObservableObject, IDisposable
    {
        public ApplicationViewModel          ApplicationViewModel { get; set; }
        public Client                        Client      { get; set; }
        public ObservableCollection<ModInfo> CurrentMods { get; set; }
        public ModInfo                       SelectedMod { get; set; }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private System.Timers.Timer _refreshTimer;

        public ReloadedApplicationViewModel(ApplicationViewModel applicationViewModel)
        {
            ApplicationViewModel = applicationViewModel;
            ApplicationViewModel.SelectedProcess.EnableRaisingEvents = true;
            ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;

            /* Try establish connection. */
            int port = ActionWrappers.TryGetValue(GetPort, 1000, 16);

            Client = new Client(port);
            Client.OnReceiveException += ClientOnOnReceiveException;
            Refresh();

            _refreshTimer = new System.Timers.Timer(100);
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += (sender, args) => Refresh();
            _refreshTimer.Enabled = true;
        }

        private void SelectedProcessOnExited(object sender, EventArgs e)
        {
            ApplicationViewModel.Page = ApplicationSubPage.ApplicationSummary;
        }

        private void ClientOnOnReceiveException(GenericExceptionResponse obj)
        {
            var box = new Pages.Dialogs.MessageBox(Errors.Error(), obj.Message);
            box.ShowDialog();
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
                if (CurrentMods != null)
                {
                    // Add new entries.
                    // In new set but not old set: loadedMods \ currentSet
                    var newMods = loadedMods.Mods.ToHashSet();
                    newMods.ExceptWith(CurrentMods.ToHashSet());

                    // Remove old entries.
                    var oldMods = CurrentMods.ToHashSet();
                    oldMods.ExceptWith(loadedMods.Mods.ToHashSet());

                    // Modify list.
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var newMod in newMods)
                            CurrentMods.Add(newMod);

                        foreach (var removedMod in oldMods)
                            CurrentMods.Remove(removedMod);
                    });
                }
                else
                {
                    CurrentMods = new ObservableCollection<ModInfo>(loadedMods.Mods);
                }
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
