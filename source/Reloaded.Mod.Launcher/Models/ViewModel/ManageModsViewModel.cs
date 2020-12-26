using System.Collections.ObjectModel;
using System.Linq;
using Reloaded.Mod.Launcher.Commands.ManageModsPage;
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

        /* If false, events to reload mod list are not sent. */
        private ApplicationConfigService _appConfigService;
        private readonly SetModImageCommand _setModImageCommand;

        public ManageModsViewModel(ApplicationConfigService appConfigService, ModConfigService modConfigService)
        {
            ModConfigService = modConfigService;
            _appConfigService = appConfigService;
            _setModImageCommand = new SetModImageCommand(this);

            SelectedModTuple = ModConfigService.Mods.FirstOrDefault();
        }

        /// <summary>
        /// Saves the old mod tuple about to be swapped out by the UI and updates the UI
        /// with the details of the new tuple.
        /// </summary>
        public void SetNewMod(PathTuple<ModConfig> oldModTuple, PathTuple<ModConfig> newModTuple)
        {
            // Save old collection.
            if (oldModTuple != null && ModConfigService.Mods.Contains(oldModTuple))
                SaveMod(oldModTuple);

            // Make new collection.
            if (newModTuple == null) 
                return;

            var supportedAppIds = newModTuple.Config.SupportedAppId;
            var tuples = _appConfigService.Applications.Select(x => new BooleanGenericTuple<ApplicationConfig>(supportedAppIds.Contains(x.Config.AppId), x.Config));
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
            if (!_setModImageCommand.CanExecute(null))
                return;

            _setModImageCommand.Execute(null);
        }
    }
}
