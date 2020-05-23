using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Reloaded.Mod.Launcher.Commands.AddAppPage;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class AddAppPage : ReloadedIIPage, IDisposable
    {
        public AddAppViewModel ViewModel { get; set; }

        public AddAppPage() : base()
        {  
            InitializeComponent();

            // Setup ViewModel
            ViewModel = IoC.Get<AddAppViewModel>();
            this.DataContext = ViewModel;
            this.AnimateOutStarted += SaveCurrentSelectedItem;
            this.AnimateOutStarted += Dispose;
            IoC.Get<MainWindow>().Closing += OnMainWindowClosing;
            this.AnimateInStarted += SetDefaultSelectionIndex;
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
        }

        private void SaveCurrentSelectedItem()
        {
            if (ViewModel.MainPageViewModel.Applications.Count >= 0)
            {
                try
                {
                    var imagePathAppTuple = ViewModel.MainPageViewModel.Applications.First(x => x.Config.Equals(ViewModel.Application.Config));
                    ApplicationConfig.WriteConfiguration(imagePathAppTuple.ConfigPath, ViewModel.Application.Config);
                }
                catch (Exception) { Debug.WriteLine("AddAppPage: Failed to save current selected item."); }
            }
        }

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Filter for first load.
            if (e.RemovedItems.Count > 0)
            {
                // Write config.
                // Without the file existence check, what can happen is we remove an application and it immediately comes back.
                var tuple = (ImageApplicationPathTuple)e.RemovedItems[0];
                if (File.Exists(tuple.ConfigPath))
                    ApplicationConfig.WriteConfiguration(tuple.ConfigPath, tuple.Config);
            }
        }

        private void Image_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.SetApplicationImageCommand.CanExecute(null))
            {
                ViewModel.SetApplicationImageCommand.Execute(null);
                ViewModel.RaiseApplicationChangedEvent();
            }
        }

        private void SetDefaultSelectionIndex() => ViewModel.SelectedIndex = 0;
        private void OnMainWindowClosing(object sender, CancelEventArgs e) => SaveCurrentSelectedItem();
    }
}
