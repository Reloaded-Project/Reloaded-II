using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Dialogs
{
    /// <summary>
    /// Interaction logic for ModSelectDialog.xaml
    /// </summary>
    public partial class LoadModSelectDialog : ReloadedWindow
    {
        public ApplicationViewModel ApplicationViewModel { get; set; }
        public ReloadedApplicationViewModel ReloadedApplicationViewModel { get; set; }
        public PathTuple<ModConfig> SelectedMod { get; set; }

        private readonly CollectionViewSource _modsViewSource;

        public LoadModSelectDialog(ApplicationViewModel applicationViewModel, ReloadedApplicationViewModel reloadedApplicationViewModel)
        {
            InitializeComponent();
            ApplicationViewModel = applicationViewModel;
            ReloadedApplicationViewModel = reloadedApplicationViewModel;

            // Setup filters
            var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
            _modsViewSource = manipulator.Get<CollectionViewSource>("FilteredMods");
            _modsViewSource.Filter += ModsViewSourceOnFilter;
        }

        /* Filtering Code */
        private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (PathTuple<ModConfig>) e.Item;
            e.Accepted = tuple.Config.ModName.Contains(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
            if (! e.Accepted)
                e.Accepted = tuple.Config.ModId.Contains(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private void ModsFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _modsViewSource.View.Refresh();
        }

        /* Select/Pick Code */
        private void LoadMod_Click(object sender, RoutedEventArgs e)
        {
            try { Task.Run(() => ReloadedApplicationViewModel.Client?.LoadMod(SelectedMod.Config.ModId)); }
            catch (Exception) { /* Ignored */ }
            this.Close();
        }
    }
}
