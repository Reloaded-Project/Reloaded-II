using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Reloaded.Mod.Launcher.Commands.Generic.Mod;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class ManageModsViewModel : ObservableObject
    {
        /* Fields for Data Binding */
        public PathTuple<ModConfig> SelectedModTuple { get; set; }
        public ObservableCollection<BooleanGenericTuple<ApplicationConfig>> EnabledAppIds { get; set; } = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>();
        public ModConfigService ModConfigService { get; set; }

        /* Commands */
        public OpenModFolderCommand OpenModFolderCommand { get; set; }
        public DeleteModCommand DeleteModCommand { get; set; }

        /* If false, events to reload mod list are not sent. */
        private ApplicationConfigService _appConfigService;
        private SetModImageCommand _setModImageCommand;

        public ManageModsViewModel(ApplicationConfigService appConfigService, ModConfigService modConfigService)
        {
            ModConfigService = modConfigService;
            _appConfigService = appConfigService;

            SelectedModTuple = ModConfigService.Items.FirstOrDefault();
            this.PropertyChanged += OnSelectedModChanged;
            UpdateCommands();
        }

        /// <summary>
        /// Saves the old mod tuple about to be swapped out by the UI and updates the UI
        /// with the details of the new tuple.
        /// </summary>
        public void SetNewMod(PathTuple<ModConfig> oldModTuple, PathTuple<ModConfig> newModTuple)
        {
            // Save old collection.
            if (oldModTuple != null && ModConfigService.Items.Contains(oldModTuple))
                SaveMod(oldModTuple);

            // Make new collection.
            if (newModTuple == null) 
                return;

            var supportedAppIds = newModTuple.Config.SupportedAppId;
            var tuples = _appConfigService.Items.Select(x => new BooleanGenericTuple<ApplicationConfig>(supportedAppIds.Contains(x.Config.AppId), x.Config));
            EnabledAppIds = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>(tuples);
        }

        /// <summary>
        /// Saves a given mod tuple to the hard disk.
        /// </summary>
        public void SaveMod(PathTuple<ModConfig> oldModTuple)
        {
            if (oldModTuple == null) 
                return;

            if (EnabledAppIds != null)
                oldModTuple.Config.SupportedAppId = EnabledAppIds.Where(x => x.Enabled).Select(x => x.Generic.AppId).ToArray();

            oldModTuple.SaveAsync();
        }
        
        /// <summary>
        /// Sets a new mod image.
        /// </summary>
        public void SetNewImage()
        {
            if (_setModImageCommand.CanExecute(null))
                _setModImageCommand.Execute(null);
        }

        private void OnSelectedModChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedModTuple))
                UpdateCommands();
        }

        private void UpdateCommands()
        {
            OpenModFolderCommand = new OpenModFolderCommand(SelectedModTuple);
            DeleteModCommand = new DeleteModCommand(SelectedModTuple);
            _setModImageCommand = new SetModImageCommand(SelectedModTuple);
        }
    }
}
