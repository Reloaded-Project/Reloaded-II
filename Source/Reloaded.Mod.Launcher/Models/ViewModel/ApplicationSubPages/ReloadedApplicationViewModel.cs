using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ReloadedApplicationViewModel : ObservableObject
    {
        public ApplicationViewModel ApplicationViewModel { get; set; }
        public Client Client            { get; set; }
        public ModInfo[]    LoadedMods  { get; set; }
        public ModInfo      SelectedMod { get; set; }

        private Task _connectToServer;
        private CancellationTokenSource _cancellationTokenSource;

        public ReloadedApplicationViewModel(ApplicationViewModel applicationViewModel)
        {
            ApplicationViewModel = applicationViewModel;
            _cancellationTokenSource = new CancellationTokenSource();

            /* Asynchronously try establish connection. */
            _connectToServer = Task.Run(() =>
            {
                int port = ActionWrappers.TryGetValue(GetPort, 1000, 16, _cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;

                Client = new Client(port);
                LoadedMods = Client.GetLoadedModsAsync().Result.Mods;
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Cancels the token used to establish server connection.
        /// </summary>
        public void CancelToken()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Retrieves the list of loaded mods.
        /// </summary>
        public void RefreshLoadedMods()
        {
            LoadedMods = Client?.GetLoadedModsAsync().Result.Mods;
        }

        /// <summary>
        /// Acquires the port to connect to the remote process.
        /// </summary>
        private int GetPort()
        {
            int pid             = ApplicationViewModel.SelectedProcess.Id;
            var mappedFile      = MemoryMappedFile.OpenExisting(ServerUtility.GetMappedFileNameForPid(pid));
            var view            = mappedFile.CreateViewStream();
            var binaryReader    = new BinaryReader(view);
            return binaryReader.ReadInt32();
        }
    }
}
