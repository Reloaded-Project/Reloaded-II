using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Launcher.Utility;
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
        public ImageModPathTuple SelectedMod { get; set; }

        private readonly CollectionViewSource _modsViewSource;

        public LoadModSelectDialog(ApplicationViewModel applicationViewModel, ReloadedApplicationViewModel reloadedApplicationViewModel)
        {
            InitializeComponent();
            ApplicationViewModel = applicationViewModel;
            ReloadedApplicationViewModel = reloadedApplicationViewModel;

            // Setup filters
            var manipulator = new ResourceManipulator(this.Contents);
            _modsViewSource = manipulator.Get<CollectionViewSource>("SortedMods");
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

            var tuple = (ImageModPathTuple) e.Item;
            e.Accepted = tuple.ModConfig.ModName.IndexOf(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
            if (! e.Accepted)
                e.Accepted = tuple.ModConfig.ModId.IndexOf(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void ModsFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _modsViewSource.View.Refresh();
        }

        /* Select/Pick Code */
        private void LoadMod_Click(object sender, RoutedEventArgs e)
        {
            try { Task.Run(() => ReloadedApplicationViewModel.Client?.LoadMod(SelectedMod.ModConfig.ModId)); }
            catch (Exception) { /* Ignored */ }
            this.Close();
        }
    }
}
