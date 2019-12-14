using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.ManageModsPage;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class ManageModsPage : ReloadedIIPage
    {
        public ManageModsViewModel ViewModel { get; set; }
        private readonly CollectionViewSource _modsViewSource;
        private readonly CollectionViewSource _appsViewSource;
        private readonly SetModImageCommand _setModImageCommand;

        public ManageModsPage() : base()
        {
            InitializeComponent();
            ViewModel = IoC.Get<ManageModsViewModel>();
            this.DataContext = ViewModel;
            this.AnimateOutStarted += SaveCurrentMod;

            // Setup filters
            var manipulator = new ResourceManipulator(this.Contents);
            _modsViewSource = manipulator.Get<CollectionViewSource>("SortedMods");
            _appsViewSource = manipulator.Get<CollectionViewSource>("SortedApps");
            _modsViewSource.Filter += ModsViewSourceOnFilter;
            _appsViewSource.Filter += AppsViewSourceOnFilter;
            _setModImageCommand = new SetModImageCommand();
        }

        private void AppsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.AppsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<ApplicationConfig>)e.Item;
            e.Accepted = tuple.Generic.AppName.IndexOf(this.AppsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (ImageModPathTuple)e.Item;
            e.Accepted = tuple.ModConfig.ModName.IndexOf(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var createModDialog = new CreateModDialog();
                createModDialog.Owner = Window.GetWindow(this);
                createModDialog.ShowDialog();
            }
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedModTuple != null)
                ViewModel.Icon = Imaging.BitmapFromUri(new Uri(ViewModel.SelectedModTuple.Image));

            // Tell viewmodel to swap ModId compatibility chart.
            ImageModPathTuple oldModTuple = null;
            ImageModPathTuple newModTuple = null;
            if (e.RemovedItems.Count > 0)
                oldModTuple = e.RemovedItems[0] as ImageModPathTuple;

            if (e.AddedItems.Count > 0)
                newModTuple = e.AddedItems[0] as ImageModPathTuple;

            ViewModel.SwapMods(oldModTuple, newModTuple);
            e.Handled = true;
        }

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _modsViewSource.View.Refresh();
        private void AppsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _appsViewSource.View.Refresh();
        private void SaveCurrentMod() => ViewModel.SaveMod(ViewModel.SelectedModTuple);

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_setModImageCommand.CanExecute(null))
            {
                _setModImageCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
