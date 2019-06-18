using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using Reloaded.Mod.Launcher.Commands;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ManageModsViewModel : ObservableObject
    {
        private const string JsonFilter = "*.json";

        /* Mod Config Loader. */
        private static ConfigReader<ModConfig> _modConfigReader = new ConfigReader<ModConfig>();
        private static MainPageViewModel _mainPageViewModel;

        /* Fired when the available mods collection changes. */
        public event NotifyCollectionChangedEventHandler ModsChanged = (sender, args) => { };

        /* Fields */
        public ImageModPathTuple SelectedModPathTuple { get; set; }
        public ImageSource Icon { get; set; }

        public ObservableCollection<BooleanGenericTuple<ApplicationConfig>> EnabledAppIds { get; set; }
        public bool MonitorNewMods { get; set; } = true;

        [DoNotNotify]
        public ObservableCollection<ImageModPathTuple> Mods
        {
            get => _mods;
            set
            {
                value.CollectionChanged += ModsChanged;
                _mods = value;

                RaisePropertyChangedEvent(nameof(Mods));
                ModsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /* Fired when the list of available mods changes. */
        private ObservableCollection<ImageModPathTuple> _mods;

        /* Get Applications Task */
        private SerialTaskCommand _getModsTaskCommand = new SerialTaskCommand();
        private FileSystemWatcher _createWatcher; 
        private FileSystemWatcher _deleteFileWatcher;
        private FileSystemWatcher _deleteDirectoryWatcher;

        public ManageModsViewModel()
        {
            GetModifications();
            _mainPageViewModel = IoC.Get<MainPageViewModel>();
            string modDirectory = LoaderConfigReader.ReadConfiguration().ModConfigDirectory;

            _createWatcher = FileSystemWatcherFactory.CreateGeneric(modDirectory, ExecuteGetModifications, FileSystemWatcherFactory.FileSystemWatcherEvents.Changed, true, "*.*");
            _deleteFileWatcher = FileSystemWatcherFactory.CreateChangeCreateDelete(modDirectory, OnDeleteFile, FileSystemWatcherFactory.FileSystemWatcherEvents.Deleted, true, JsonFilter);
            _deleteDirectoryWatcher = FileSystemWatcherFactory.CreateChangeCreateDelete(modDirectory, OnDeleteDirectory, FileSystemWatcherFactory.FileSystemWatcherEvents.Deleted, false, "*.*");
            InvokeAsync(() => Icon = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.Absolute)));
        }

        public void SwapMods(ImageModPathTuple oldModTuple, ImageModPathTuple newModTuple)
        {
            // Save old collection.
            if (oldModTuple != null)
            {
                if (EnabledAppIds != null)
                {
                    var supportedApps = new List<string>();
                    foreach (var booleanAppTuple in EnabledAppIds)
                    {
                        if (booleanAppTuple.Enabled)
                            supportedApps.Add(booleanAppTuple.Generic.AppId);
                    }

                    oldModTuple.ModConfig.SupportedAppId = supportedApps.ToArray();
                }

                // Make sure not to refresh the collection, we will lose our index.
                // Note: Saving regardless of action because of possible other changes.
                InvokeWithoutMonitoringMods(() => oldModTuple.Save());
            }

            // Make new collection.
            if (newModTuple != null)
            {
                // Make new collection.
                var booleanAppTuples = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>();
                string[] supportedAppIds = newModTuple.ModConfig.SupportedAppId;
                foreach (var applicationPathTuple in _mainPageViewModel.Applications)
                {
                    bool enabled = supportedAppIds.Contains(applicationPathTuple.ApplicationConfig.AppId);
                    booleanAppTuples.Add(new BooleanGenericTuple<ApplicationConfig>(enabled, applicationPathTuple.ApplicationConfig));
                }
                EnabledAppIds = booleanAppTuples;
            }
        }

        public void InvokeWithoutMonitoringMods(Action action)
        {
            MonitorNewMods = false;
            action();
            MonitorNewMods = true;
        }

        private void InvokeAsync(Action action)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.InvokeAsync(action);
            else
                action();
        }

        private void ExecuteGetModifications()
        {
            if (MonitorNewMods)
                _getModsTaskCommand.Execute(new Action<CancellationToken>(GetModifications));
        }

        private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
        {
            if (MonitorNewMods)
            {
                _getModsTaskCommand.Execute(new Action<CancellationToken>(token =>
                {
                    // Remove any mod that may have been inside removed directory.
                    var allMods = Mods;
                    foreach (var mod in allMods)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (Path.GetDirectoryName(mod.ModConfigPath).Equals(e.FullPath))
                        {
                            InvokeAsync(() => Mods.Remove(mod));
                            break;
                        }
                    }
                }));
            }
        }

        private void OnDeleteFile(object sender, FileSystemEventArgs e)
        {
            if (MonitorNewMods)
            {
                _getModsTaskCommand.Execute(new Action<CancellationToken>(token =>
                {
                    // Remove any mod with matching filename to removed instance.
                    var allMods = Mods;
                    foreach (var mod in allMods)
                    {
                        if (mod.ModConfigPath.Equals(e.FullPath))
                        {
                            InvokeAsync(() => Mods.Remove(mod));
                            break;
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// Populates the application list governed by <see cref="Mods"/>.
        /// </summary>
        private void GetModifications(CancellationToken cancellationToken = default)
        {
            var mods = new ObservableCollection<ImageModPathTuple>();
            var loaderConfig = LoaderConfigReader.ReadConfiguration();
            List<PathGenericTuple<ModConfig>> modConfigs;

            // Check for cancellation request before config reading begins if necessary. 
            if (cancellationToken.IsCancellationRequested)
                return;

            // Try read all configs, this action may sometimes fail if some of the files are still being copied.
            // Worth noting is that the last fired event will never collide here and fail, thus this is a safe point to exit.
            try { modConfigs = _modConfigReader.ReadConfigurations(loaderConfig.ModConfigDirectory, ModConfig.ConfigFileName); }
            catch (Exception) { return; }

            foreach (var config in modConfigs)
                mods.Add(new ImageModPathTuple(GetImageForModConfig(config), config.Object, config.Path));

            Mods = mods;
        }

        /// <summary>
        /// Obtains an image to represent a given mod.
        /// The image is either a custom one or the default placeholder.
        /// </summary>
        private string GetImageForModConfig(PathGenericTuple<ModConfig> modConfig)
        {
            // Check if custom icon exists.
            if (!String.IsNullOrEmpty(modConfig.Object.ModIcon))
            {
                string logoDirectory = Path.GetDirectoryName(modConfig.Path);
                string logoFilePath = Path.Combine(logoDirectory, modConfig.Object.ModIcon);

                if (File.Exists(logoFilePath))
                    return logoFilePath;
            }
            
            return Paths.PLACEHOLDER_IMAGE;
        }
    }
}
