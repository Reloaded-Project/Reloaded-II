using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for SetDependenciesDialog.xaml
    /// </summary>
    public partial class SetDependenciesDialog : ReloadedWindow
    {
        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();
        public ManageModsViewModel ManageModsViewModel { get; }
        public ImageModPathTuple CurrentMod { get; }

        private readonly CollectionViewSource _dependenciesViewSource;

        public SetDependenciesDialog(ManageModsViewModel manageModsViewModel)
        {
            InitializeComponent();
            ManageModsViewModel = manageModsViewModel;
            CurrentMod = manageModsViewModel.SelectedModTuple;

            PopulateDependencies();
            this.Closed += OnClosed;

            // Setup filters
            var manipulator = new ResourceManipulator(this.Contents);
            _dependenciesViewSource = manipulator.Get<CollectionViewSource>("SortedDependencies");
            _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
        }

        private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<IModConfig>)e.Item;
            e.Accepted = tuple.Generic.ModName.IndexOf(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
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

        private void OnClosed(object sender, EventArgs e)
        {
            CurrentMod.ModConfig.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
            CurrentMod.Save();
        }

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _dependenciesViewSource.View.Refresh();
        }
    }
}
