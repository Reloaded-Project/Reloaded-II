using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Commands.ApplicationConfigurationPage;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ApplicationSummaryViewModel : ObservableObject, IDisposable
    {
        public ObservableCollection<BooleanGenericTuple<ImageModPathTuple>> AllMods { get; set; }
        public BooleanGenericTuple<ImageModPathTuple> SelectedMod { get; set; }
        public ImageApplicationPathTuple ApplicationTuple { get; set; }

        public OpenModFolderCommand OpenModFolderCommand { get; set; }
        public ConfigureModCommand ConfigureModCommand { get; set; }

        public ImageSource Icon { get; set; }

        public ApplicationSummaryViewModel(ApplicationViewModel model)
        {
            ApplicationTuple = model.ApplicationTuple;
            OpenModFolderCommand = new OpenModFolderCommand(this);
            ConfigureModCommand = new ConfigureModCommand(this);

            // Wait for parent to fully initialize.
            this.PropertyChanged += UpdateIcon;

            AllMods = new ObservableCollection<BooleanGenericTuple<ImageModPathTuple>>(GetInitialModSet(model, ApplicationTuple));
            AllMods.CollectionChanged += (sender, args) => SaveApplication(); // Save on reorder.
        }

        ~ApplicationSummaryViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            OpenModFolderCommand?.Dispose();
            ConfigureModCommand?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Builds the initial set of mods to display in the list.
        /// </summary>
        private List<BooleanGenericTuple<ImageModPathTuple>> GetInitialModSet(ApplicationViewModel model, ImageApplicationPathTuple applicationTuple)
        {
            // Note: Must put items in top to bottom load order.
            var enabledModIds   = applicationTuple.Config.EnabledMods;
            var modsForThisApp  = model.ModsForThisApp.ToArray();

            // Get dictionary of mods by Mod ID
            var modDictionary  = new Dictionary<string, ImageModPathTuple>();
            foreach (var mod in modsForThisApp)
                modDictionary[mod.ModConfig.ModId] = mod;

            // Add enabled mods.
            var totalModList = new List<BooleanGenericTuple<ImageModPathTuple>>(modsForThisApp.Length);
            foreach (var enabledModId in enabledModIds)
            {
                if (modDictionary.ContainsKey(enabledModId))
                    totalModList.Add(new BooleanGenericTuple<ImageModPathTuple>(true, modDictionary[enabledModId]));
            }

            // Add disabled mods.
            var enabledModIdSet = applicationTuple.Config.EnabledMods.ToHashSet();
            var disabledMods    = modsForThisApp.Where(x => !enabledModIdSet.Contains(x.ModConfig.ModId));
            totalModList.AddRange(disabledMods.Select(x => new BooleanGenericTuple<ImageModPathTuple>(false, x)));
            return totalModList;
        }

        private BooleanGenericTuple<ImageModPathTuple> MakeSaveSubscribedGenericTuple(bool isEnabled, ImageModPathTuple item)
        {
            // Make BooleanGenericTuple that saves application on Enabled change.
            var tuple = new BooleanGenericTuple<ImageModPathTuple>(isEnabled, item);
            tuple.PropertyChanged += SaveOnEnabledPropertyChanged;
            return tuple;
        }

        // == Events ==
        private void SaveOnEnabledPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BooleanGenericTuple<IModConfig>.NameOfEnabled)
                SaveApplication();
        }

        private void SaveApplication()
        {
            ApplicationTuple.Config.EnabledMods = AllMods.Where(x => x.Enabled).Select(x => x.Generic.ModConfig.ModId).ToArray();
            ApplicationTuple.Save();
        }

        private void UpdateIcon(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedMod))
            {
                if (SelectedMod?.Generic != null)
                {
                    Icon = Imaging.BitmapFromUri(new Uri(SelectedMod.Generic.Image));
                }
            }
        }
    }
}
