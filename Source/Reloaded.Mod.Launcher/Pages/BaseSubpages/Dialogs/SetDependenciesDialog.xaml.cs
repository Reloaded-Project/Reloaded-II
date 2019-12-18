using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Weaving;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for SetDependenciesDialog.xaml
    /// </summary>
    public partial class SetDependenciesDialog : ReloadedWindow
    {
        public SetDependenciesDialogViewmodel ViewModel { get; set; }

        public SetDependenciesDialog(ManageModsViewModel manageModsViewModel)
        {
            InitializeComponent();
            ViewModel = new SetDependenciesDialogViewmodel(manageModsViewModel, new ResourceManipulator(this.Contents));
            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e) => ViewModel.SaveCurrentMod();
        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ViewModel.RefreshModList();
    }

    public class SetDependenciesDialogViewmodel : ObservableObject
    {
        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();
        public ManageModsViewModel ManageModsViewModel { get; }
        public ImageModPathTuple CurrentMod { get; }

        public string ModsFilter { get; set; } = "";
        private readonly CollectionViewSource _dependenciesViewSource;

        public SetDependenciesDialogViewmodel(ManageModsViewModel manageModsViewModel, ResourceManipulator manipulator)
        {
            ManageModsViewModel = manageModsViewModel;
            CurrentMod = ManageModsViewModel.SelectedModTuple;

            _dependenciesViewSource = manipulator.Get<CollectionViewSource>("SortedDependencies");
            _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
            PopulateDependencies();
        }

        public void RefreshModList() => _dependenciesViewSource.View.Refresh();

        public void SaveCurrentMod()
        {
            CurrentMod.ModConfig.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
            CurrentMod.Save();
        }

        private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<IModConfig>)e.Item;
            e.Accepted = tuple.Generic.ModName.IndexOf(this.ModsFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void PopulateDependencies()
        {
            var mods = ManageModsViewModel.Mods; // In case collection changes during window open.
            foreach (var mod in mods)
            {
                bool enabled = CurrentMod.ModConfig.ModDependencies.Contains(mod.ModConfig.ModId);
                Dependencies.Add(new BooleanGenericTuple<IModConfig>(enabled, mod.ModConfig));
            }
        }
    }
}
