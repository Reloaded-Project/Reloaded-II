using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.DesignTimeModel;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class ManageModsPage : ReloadedIIPage
    {
        public ManageModsViewModel ViewModel { get; set; }

        public ManageModsPage() : base()
        {  
            InitializeComponent();
            ViewModel = IoC.Get<ManageModsViewModel>();
            this.DataContext = ViewModel;
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
            if (ViewModel.SelectedModPathTuple != null)
                ViewModel.Icon = Imaging.BitmapFromUri(new Uri(ViewModel.SelectedModPathTuple.Image));

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
    }
}
