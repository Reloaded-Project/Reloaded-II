using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;
using Reloaded.WPF.Utilities;
using static Reloaded.Mod.Launcher.Utility.ActionWrappers;
using static Reloaded.Mod.Loader.IO.FileSystemWatcherFactory;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ManageModsViewModel : ObservableObject
    {
        /* Mod Config Loader. */
        private static MainPageViewModel _mainPageViewModel;

        /* Fired when the available mods collection changes. */
        public event NotifyCollectionChangedEventHandler ModsChanged = (sender, args) => { };
        public event Action<ImageModPathTuple> ModSaving = tuple => { }; // When a mod is about to be saved.

        /* Fields for Data Binding */
        public ImageModPathTuple SelectedModTuple { get; set; }
        public ImageSource Icon { get; set; }
        public ObservableCollection<BooleanGenericTuple<ApplicationConfig>> EnabledAppIds { get; set; }

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
        private ObservableCollection<ImageModPathTuple> _mods = new ObservableCollection<ImageModPathTuple>();

        /* If false, events to reload mod list are not sent. */
        private bool _monitorNewMods = true;

        /* Get Applications Task */
        private CancellableExecuteActionTimer _getApplicationsActionTimer = new CancellableExecuteActionTimer(new XamlResource<int>("RefreshModsEventTickTimer").Get());
        private readonly FileSystemWatcher _createWatcher; 
        private readonly FileSystemWatcher _deleteFileWatcher;
        private readonly FileSystemWatcher _deleteDirectoryWatcher;

        public ManageModsViewModel(MainPageViewModel mainPageViewModel, LoaderConfig loaderConfig)
        {
            GetModifications();
            _mainPageViewModel = mainPageViewModel;
            string modDirectory = loaderConfig.ModConfigDirectory;

            _createWatcher = CreateGeneric(modDirectory, ExecuteGetModifications, FileSystemWatcherEvents.Changed | FileSystemWatcherEvents.Created, true, "*.*");
            _deleteFileWatcher = CreateChangeCreateDelete(modDirectory, OnDeleteFile, FileSystemWatcherEvents.Deleted);
            _deleteDirectoryWatcher = CreateChangeCreateDelete(modDirectory, OnDeleteDirectory, FileSystemWatcherEvents.Deleted, false, "*.*");
            ExecuteWithApplicationDispatcherAsync(() => Icon = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.Absolute)));
        }

        /// <summary>
        /// Saves the old mod tuple about to be swapped out by the UI and updates the UI
        /// with the details of the new tuple.
        /// </summary>
        public void SwapMods(ImageModPathTuple oldModTuple, ImageModPathTuple newModTuple)
        {
            // Save old collection.
            if (oldModTuple != null)
                SaveMod(oldModTuple);

            // Make new collection.
            if (newModTuple == null) 
                return;

            var supportedAppIds = newModTuple.ModConfig.SupportedAppId;
            var tuples = _mainPageViewModel.Applications.Select(x => new BooleanGenericTuple<ApplicationConfig>(supportedAppIds.Contains(x.ApplicationConfig.AppId), x.ApplicationConfig));
            EnabledAppIds = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>(tuples);
        }

        /// <summary>
        /// Saves a given mod tuple to the hard disk.
        /// </summary>
        public void SaveMod(ImageModPathTuple oldModTuple)
        {
            if (oldModTuple == null) 
                return;

            if (EnabledAppIds != null)
                oldModTuple.ModConfig.SupportedAppId = EnabledAppIds.Where(x => x.Enabled).Select(x => x.Generic.AppId).ToArray();

            //InvokeWithoutMonitoringMods(oldModTuple.Save);
            oldModTuple.Save();
            ModSaving(oldModTuple);
        }

        /// <summary>
        /// Obtains an image to represent a given mod, either a custom one or the default placeholder.
        /// </summary>
        public string GetImageForModConfig(PathGenericTuple<ModConfig> modConfig)
        {
            return ModConfig.TryGetIconPath(modConfig.Path, modConfig.Object, out string iconPath) ? iconPath : Paths.PLACEHOLDER_IMAGE;
        }

        /// <summary>
        /// Populates the mod list governed by <see cref="Mods"/>.
        /// </summary>
        private void GetModifications(CancellationToken cancellationToken = default)
        {
            try
            {
                var modConfigs = ModConfig.GetAllMods(IoC.Get<LoaderConfig>().ModConfigDirectory, cancellationToken);
                var mods = modConfigs.Select(x => new ImageModPathTuple(GetImageForModConfig(x), x.Object, x.Path));
                ExecuteWithApplicationDispatcher(() => Collections.ModifyObservableCollection(Mods, mods));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Executes a function while temporarily disabling the automatic file update events.
        /// </summary>
        public void InvokeWithoutMonitoringMods(Action action)
        {
            _monitorNewMods = false;
            action();
            _monitorNewMods = true;
        }

        // == Events ==
        private void ExecuteGetModifications()
        {
            if (_monitorNewMods)
                _getApplicationsActionTimer.SetActionAndReset(GetModifications);
        }

        private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
        {
            // Handles deleted directories containing mod configurations.
            // Remove any mod that may have been inside removed directory.
            if (_monitorNewMods)
            {
                ExecuteWithApplicationDispatcher(() =>
                {
                    // Copy in case Mods collection changes.
                    var allMods     = Mods; 
                    var deletedMods = allMods.Where(x => Path.GetDirectoryName(x.ModConfigPath).Equals(e.FullPath));
                    ExecuteWithApplicationDispatcherAsync(() =>
                    {
                        foreach (var deletedMod in deletedMods) 
                            Mods.Remove(deletedMod);
                    });
                });
            }
        }

        private void OnDeleteFile(object sender, FileSystemEventArgs e)
        {
            // Handles deleted mod configuration files.
            // Remove any mod that matches the path of a deleted config file.
            if (_monitorNewMods)
            {
                ExecuteWithApplicationDispatcher(() =>
                {
                    // Copy in case Mods collection changes.
                    var allMods     = Mods;
                    var deletedMods = allMods.Where(x => x.ModConfigPath.Equals(e.FullPath));
                    ExecuteWithApplicationDispatcherAsync(() =>
                    {
                        foreach (var deletedMod in deletedMods)
                            Mods.Remove(deletedMod);
                    });
                });
            }
        }
    }
}
