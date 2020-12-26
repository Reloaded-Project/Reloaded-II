using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using static Reloaded.Mod.Loader.IO.Utility.FileSystemWatcherFactory;

namespace Reloaded.Mod.Loader.IO.Services
{
    /// <summary>
    /// Service which provides access to various mod configurations.
    /// </summary>
    public class ModConfigService
    {
        /// <summary>
        /// Current up to date list of all applications.
        /// </summary>
        public ObservableCollection<PathTuple<ModConfig>> Mods { get; set; } = new ObservableCollection<PathTuple<ModConfig>>();

        /// <summary>
        /// The folder containing all of the folders of application profiles.
        /// </summary>
        public string ConfigDirectory { get; }

        /* Mod Monitoring */
        private readonly TaskScheduler _getModsScheduler = new TaskScheduler(100);
        private readonly FileSystemWatcher _createWatcher;
        private readonly FileSystemWatcher _deleteFileWatcher;
        private readonly FileSystemWatcher _deleteDirectoryWatcher;
        private SynchronizationContext _context = SynchronizationContext.Current;

        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        public ModConfigService(LoaderConfig config, SynchronizationContext context = null)
        {
            _context = context ?? _context;

            ConfigDirectory = config.ModConfigDirectory;
            _createWatcher = CreateGeneric(ConfigDirectory, () => _getModsScheduler.Schedule(GetModifications), FileSystemWatcherEvents.Changed | FileSystemWatcherEvents.Created);
            _deleteFileWatcher = CreateChangeCreateDelete(ConfigDirectory, OnDeleteFile, FileSystemWatcherEvents.Deleted);
            _deleteDirectoryWatcher = CreateChangeCreateDelete(ConfigDirectory, OnDeleteDirectory, FileSystemWatcherEvents.Deleted, false, "*.*");
            GetModifications();
        }

        /*
        /// <summary>
        /// Attempts to move the folder used for storing mod profiles to a new directory.
        /// </summary>
        /// <param name="newDirectory">Path to the new directory.</param>
        public void MoveFolder(string newDirectory)
        {

        }
        */

        /// <summary>
        /// Populates the mod list governed by <see cref="Mods"/>.
        /// </summary>
        private void GetModifications(CancellationToken cancellationToken = default)
        {
            try
            {
                var modConfigs = ModConfig.GetAllMods(ConfigDirectory, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;

                _context.Post(() => Collections.ModifyObservableCollection(Mods, modConfigs));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
        {
            var allMods = Mods;
            var deletedMod = allMods.FirstOrDefault(x => x.Path.Equals(e.FullPath));

            if (deletedMod != null)
                _context.Post(() => Mods.Remove(deletedMod));
        }

        private void OnDeleteFile(object sender, FileSystemEventArgs e)
        {
            var allApps = Mods;
            var deletedApp = allApps.First(x => Path.GetDirectoryName(x.Path).Equals(e.FullPath));
            if (deletedApp != null)
                _context.Post(() => Mods.Remove(deletedApp));
        }
    }
}
