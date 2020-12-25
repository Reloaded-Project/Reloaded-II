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

        private void SaveCurrentSelectedItem() => ViewModel.SaveSelectedItem();

        private void Image_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ViewModel.SetAppImage();

        private void OnMainWindowClosing(object sender, CancelEventArgs e) => SaveCurrentSelectedItem();
    }
}
