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
    /// Service which provides access to various application configurations.
    /// </summary>
    public class ApplicationConfigService : ObservableObject
    {
        /// <summary>
        /// Current up to date list of all applications.
        /// </summary>
        public ObservableCollection<PathTuple<ApplicationConfig>> Applications { get; set; } = new ObservableCollection<PathTuple<ApplicationConfig>>();

        /// <summary>
        /// The folder containing all of the folders of application profiles.
        /// </summary>
        public string ConfigDirectory { get; }

        /* Application Monitoring */
        private readonly TaskScheduler _getApplicationsScheduler = new TaskScheduler(100);
        private readonly FileSystemWatcher _createWatcher;
        private readonly FileSystemWatcher _deleteFileWatcher;
        private readonly FileSystemWatcher _deleteDirectoryWatcher;
        private SynchronizationContext _context = SynchronizationContext.Current;

        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        public ApplicationConfigService(LoaderConfig config, SynchronizationContext context = null)
        {
            _context = context ?? _context;

            ConfigDirectory = config.ApplicationConfigDirectory;
            _createWatcher = CreateGeneric(ConfigDirectory, () => _getApplicationsScheduler.Schedule(GetApplications), FileSystemWatcherEvents.Changed | FileSystemWatcherEvents.Created);
            _deleteFileWatcher = CreateChangeCreateDelete(ConfigDirectory, OnDeleteFile, FileSystemWatcherEvents.Deleted);
            _deleteDirectoryWatcher = CreateChangeCreateDelete(ConfigDirectory, OnDeleteDirectory, FileSystemWatcherEvents.Deleted, false, "*.*");
            GetApplications();
        }

        /*
        /// <summary>
        /// Attempts to move the folder used for storing application profiles to a new directory.
        /// </summary>
        /// <param name="newDirectory">Path to the new directory.</param>
        public void MoveFolder(string newDirectory)
        {

        }
        */

        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications(CancellationToken cancellationToken = default)
        {
            try
            {
                var appConfigs = ApplicationConfig.GetAllApplications(ConfigDirectory, cancellationToken);
                if (cancellationToken.IsCancellationRequested) 
                    return;

                _context.Post(() => Collections.ModifyObservableCollection(Applications, appConfigs));
            }
            catch (Exception) { }
        }

        private void OnDeleteFile(object sender, FileSystemEventArgs e)
        {
            // Handles deleted mod configuration files.
            // Remove any mod that matches the path of a deleted config file.
            var allApps    = Applications;
            var deletedApp = allApps.FirstOrDefault(x => x.Path.Equals(e.FullPath));
                
            if (deletedApp != null)
                _context.Post(() => Applications.Remove(deletedApp));
        }

        private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
        {
            // Handles deleted directories containing mod configurations.
            // Remove any mod that may have been inside removed directory.
            var allApps     = Applications;
            var deletedApp  = allApps.First(x => Path.GetDirectoryName(x.Path).Equals(e.FullPath));
            if (deletedApp != null)
                _context.Post(() => Applications.Remove(deletedApp));
        }
    }
}