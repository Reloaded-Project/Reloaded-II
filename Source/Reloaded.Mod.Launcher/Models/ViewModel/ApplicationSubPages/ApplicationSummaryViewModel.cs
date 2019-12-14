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
using Rock.Collections;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ApplicationSummaryViewModel : ObservableObject, IDisposable
    {
        public ObservableCollection<BooleanGenericTuple<ImageModPathTuple>> AllMods { get; set; }
        public BooleanGenericTuple<ImageModPathTuple> SelectedMod { get; set; }
        public ImageApplicationPathTuple ApplicationTuple { get; set; }

        public OpenModFolderCommand OpenModFolderCommand { get; set; }
        public ConfigureModCommand ConfigureModCommand { get; set; }

        public int SelectedModIndex { get; set; } = 0;
        public ImageSource Icon { get; set; }

        public ApplicationSummaryViewModel(ApplicationViewModel model)
        {
            ApplicationTuple = model.ApplicationTuple;
            OpenModFolderCommand = new OpenModFolderCommand(this);
            ConfigureModCommand = new ConfigureModCommand(this);

            // Wait for parent to fully initialize.
            model.InitializeClassTask.Wait();
            this.PropertyChanged += OnSelectedModChanged;

            var enabledModList = GetInitialModSet(model, ApplicationTuple);
            AllMods = new ObservableCollection<BooleanGenericTuple<ImageModPathTuple>>(enabledModList);
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

        private void OnSelectedModChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedMod))
            {
                if (SelectedMod?.Generic != null)
                {
                    Icon = Imaging.BitmapFromUri(new Uri(SelectedMod.Generic.Image));
                }
            }
        }

        /// <summary>
        /// Builds the initial set of mods to display in the list.
        /// </summary>
        private List<BooleanGenericTuple<ImageModPathTuple>> GetInitialModSet(ApplicationViewModel model, ImageApplicationPathTuple applicationTuple)
        {
            // Note: Must put items in top to bottom load order.
            var enabledMods = applicationTuple.Config.EnabledMods;
            var modsForThisApp = model.ModsForThisApp.ToArray();

            // Build set of enabled mods in order of load | O(N^2)
            var enabledModSet = new OrderedHashSet<ImageModPathTuple>(modsForThisApp.Length);
            foreach (var enabledMod in enabledMods)
            {
                foreach (var modForThisApp in modsForThisApp)
                {
                    var modConfig = modForThisApp;
                    if (modConfig.ModConfig.ModId == enabledMod)
                    {
                        enabledModSet.Add(modConfig);
                        break;
                    }
                }
            }

            var totalModList = new List<BooleanGenericTuple<ImageModPathTuple>>(modsForThisApp.Length);
            foreach (var mod in enabledModSet)
                totalModList.Add(MakeSaveSubscribedGenericTuple(true, mod));

            // Now add all items not in set.
            foreach (var mod in modsForThisApp)
            {
                if (! enabledModSet.Contains(mod))
                    totalModList.Add(MakeSaveSubscribedGenericTuple(false, mod));
            }

            return totalModList;
        }

        /* Make BooleanGenericTuple that saves application on Enabled change. */
        private BooleanGenericTuple<ImageModPathTuple> MakeSaveSubscribedGenericTuple(bool isEnabled, ImageModPathTuple item)
        {
            var tuple = new BooleanGenericTuple<ImageModPathTuple>(isEnabled, item);
            tuple.PropertyChanged += SaveOnEnabledPropertyChanged;
            return tuple;
        }


        private void SaveOnEnabledPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BooleanGenericTuple<IModConfig>.NameOfEnabled)
                SaveApplication();
        }

        /* Application Save Implementation */
        private void SaveApplication()
        {
            // Note: Do not use LINQ, too slow for our needs.
            // Note 2: Top to bottom load order.
            List<string> enabledMods = new List<string>(AllMods.Count);
            foreach (var mod in AllMods)
            {
                if (mod.Enabled)
                    enabledMods.Add(mod.Generic.ModConfig.ModId);
            }

            ApplicationTuple.Config.EnabledMods = enabledMods.ToArray();
            ApplicationTuple.Save();
        }
    }
}
