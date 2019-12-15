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
        
        /// <summary>
        /// Fired when any individual mod of the mods collection changes.
        /// This is fired only when the contents of any of the mods change compared to what is internally stored,
        /// i.e. file was edited by user outside of the program.
        /// </summary>
        public event NotifyCollectionChangedEventHandler ModChanged = (sender, args) => { };

        /// <summary>
        /// Executed after a change in external files was detected and new files were read-in.
        /// This is essentially the same as <see cref="ModChanged"/>, except it is ran per-scan instead of per-file.
        /// Unlike <see cref="ModChanged"/> however, this always runs regardless of whether mods were changed or not.
        /// </summary>
        public event Action OnGetModifications = () => { }; // 

        /* Fields for Data Binding */
        public ImageModPathTuple SelectedModTuple { get; set; }
        public ImageSource Icon { get; set; }
        public ObservableCollection<BooleanGenericTuple<ApplicationConfig>> EnabledAppIds { get; set; }
        public ObservableCollection<ImageModPathTuple> Mods { get; set; } 

        /* If false, events to reload mod list are not sent. */
        private bool _monitorNewMods = true;

        /* Get Applications Task */
        private CancellableExecuteActionTimer _getApplicationsActionTimer = new CancellableExecuteActionTimer(new XamlResource<int>("RefreshModsEventTickTimer").Get());
        private readonly FileSystemWatcher _createWatcher; 
        private readonly FileSystemWatcher _deleteFileWatcher;
        private readonly FileSystemWatcher _deleteDirectoryWatcher;

        public ManageModsViewModel(MainPageViewModel mainPageViewModel, LoaderConfig loaderConfig)
        {
            Mods = new ObservableCollection<ImageModPathTuple>();
            Mods.CollectionChanged += (sender, args) =>
            {
                // Hack: Reason is if we load a mod set, enable mods for app and go back to main menu and old app is highlighted, stuff will get overwritten.
                SelectedModTuple = null;
                ModChanged(sender, args);
            };
            GetModifications();

            _mainPageViewModel = mainPageViewModel;
            string modDirectory = loaderConfig.ModConfigDirectory;

            _createWatcher = CreateGeneric(modDirectory, ExecuteGetModifications, FileSystemWatcherEvents.Changed | FileSystemWatcherEvents.Created);
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
            var tuples = _mainPageViewModel.Applications.Select(x => new BooleanGenericTuple<ApplicationConfig>(supportedAppIds.Contains(x.Config.AppId), x.Config));
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

            oldModTuple.Save();
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
                if (!cancellationToken.IsCancellationRequested)
                {
                    var mods = modConfigs.Select(x => new ImageModPathTuple(GetImageForModConfig(x), x.Object, x.Path));
                    ExecuteWithApplicationDispatcher(() => Collections.ModifyObservableCollection(Mods, mods));
                    OnGetModifications();
                }
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

                    if (deletedMods.Any())
                        OnGetModifications();
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

                    if (deletedMods.Any())
                        OnGetModifications();
                });
            }
        }
    }
}
