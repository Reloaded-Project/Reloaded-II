using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Commands.ApplicationConfigurationPage;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ApplicationSummaryViewModel : ObservableObject, IDisposable
    {
        public ObservableCollection<ModEntry> AllMods { get; set; }
        public ModEntry SelectedMod { get; set; }
        public PathTuple<ApplicationConfig> ApplicationTuple { get; set; }

        public OpenModFolderCommand OpenModFolderCommand { get; set; }
        public ConfigureModCommand ConfigureModCommand { get; set; }

        private ApplicationViewModel _applicationViewModel;

        public ApplicationSummaryViewModel(ApplicationViewModel model)
        {
            ApplicationTuple = model.ApplicationTuple;
            OpenModFolderCommand = new OpenModFolderCommand(this);
            ConfigureModCommand = new ConfigureModCommand(this);
            _applicationViewModel = model;

            // Wait for parent to fully initialize.
            _applicationViewModel.OnGetModsForThisApp += BuildModList;
            _applicationViewModel.OnLoadModSet += BuildModList;
            BuildModList();

            SelectedMod = AllMods.FirstOrDefault();
        }

        ~ApplicationSummaryViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            _applicationViewModel.OnLoadModSet -= BuildModList;
            _applicationViewModel.OnGetModsForThisApp -= BuildModList;
            OpenModFolderCommand?.Dispose();
            ConfigureModCommand?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Builds the list of mods displayed to the user.
        /// </summary>
        private void BuildModList()
        {
            AllMods = new ObservableCollection<ModEntry>(GetInitialModSet(_applicationViewModel, ApplicationTuple));
            AllMods.CollectionChanged += (sender, args) => SaveApplication(); // Save on reorder.
        }

        /// <summary>
        /// Builds the initial set of mods to display in the list.
        /// </summary>
        private List<ModEntry> GetInitialModSet(ApplicationViewModel model, PathTuple<ApplicationConfig> applicationTuple)
        {
            // Note: Must put items in top to bottom load order.
            var enabledModIds   = applicationTuple.Config.EnabledMods;
            var modsForThisApp  = model.ModsForThisApp.ToArray();

            // Get dictionary of mods for this app by Mod ID
            var modDictionary  = new Dictionary<string, PathTuple<ModConfig>>();
            foreach (var mod in modsForThisApp)
                modDictionary[mod.Config.ModId] = mod;

            // Add enabled mods.
            var totalModList = new List<ModEntry>(modsForThisApp.Length);
            foreach (var enabledModId in enabledModIds)
            {
                if (modDictionary.ContainsKey(enabledModId))
                    totalModList.Add(MakeSaveSubscribedModEntry(true, modDictionary[enabledModId]));
            }

            // Add disabled mods.
            var enabledModIdSet = applicationTuple.Config.EnabledMods.ToHashSet();
            var disabledMods    = modsForThisApp.Where(x => !enabledModIdSet.Contains(x.Config.ModId));
            totalModList.AddRange(disabledMods.Select(x => MakeSaveSubscribedModEntry(false, x)));
            return totalModList;
        }

        private ModEntry MakeSaveSubscribedModEntry(bool? isEnabled, PathTuple<ModConfig> item)
        {
            // Make BooleanGenericTuple that saves application on Enabled change.
            var tuple = new ModEntry(isEnabled, item);
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
            ApplicationTuple.Config.EnabledMods = AllMods.Where(x => x.Enabled == true).Select(x => x.Tuple.Config.ModId).ToArray();
            ApplicationTuple.Save();
        }
    }
}
