using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class EditAppPage : ReloadedIIPage, IDisposable
    {
        public EditAppViewModel ViewModel { get; set; }

        public EditAppPage() : base()
        {  
            InitializeComponent();

            // Setup ViewModel
            ViewModel = IoC.Get<EditAppViewModel>();
            this.DataContext = ViewModel;
            this.AnimateOutStarted += SaveCurrentSelectedItem;
            this.AnimateOutStarted += Dispose;
            IoC.Get<MainWindow>().Closing += OnMainWindowClosing;
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
            IoC.Get<MainWindow>().Closing -= OnMainWindowClosing;
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

        private void Image_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.SetApplicationImageCommand.CanExecute(null))
            {
                ViewModel.SetApplicationImageCommand.Execute(null);
                ViewModel.RaiseApplicationChangedEvent();
            }
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e) => SaveCurrentSelectedItem();
    }
}
