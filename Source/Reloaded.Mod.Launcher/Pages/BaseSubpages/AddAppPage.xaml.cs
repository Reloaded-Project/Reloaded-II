using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Commands.AddAppPage;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class AddAppPage : ReloadedIIPage
    {
        public AddAppViewModel ViewModel { get; set; }
        private readonly SetApplicationImageCommand _setApplicationImageCommand;

        public AddAppPage() : base()
        {  
            InitializeComponent();

            // Setup ViewModel
            ViewModel = IoC.Get<AddAppViewModel>();
            this.DataContext = ViewModel;
            this.AnimateOutStarted += SaveCurrentSelectedItem;
            this.AnimateInStarted += SetDefaultSelectionIndex;

            _setApplicationImageCommand = new SetApplicationImageCommand();
        }

        private void SaveCurrentSelectedItem()
        {
            // Saves the current selection before exiting launcher.
            Task.Run(() =>
            {
                if (ViewModel.MainPageViewModel.Applications.Count >= 0)
                {
                    try
                    {
                        var imagePathAppTuple = ViewModel.MainPageViewModel.Applications.First(x => x.ApplicationConfig.Equals(ViewModel.Application));
                        ApplicationConfig.WriteConfiguration(imagePathAppTuple.ApplicationConfigPath, ViewModel.Application);
                    }
                    catch (Exception) { Debug.WriteLine("AddAppPage: Failed to save current selected item."); }
                }
            });
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Filter for first load.
            if (e.RemovedItems.Count > 0)
            {
                // Backup and disable monitor status.
                bool oldMonitorNewApplications = ViewModel.MainPageViewModel.MonitorNewApplications;
                ViewModel.MainPageViewModel.MonitorNewApplications = false;

                // Write config.
                // Without the file existence check, what can happen is we remove an application and it immediately comes back.
                var tuple = (ImageApplicationPathTuple)e.RemovedItems[0];
                if (File.Exists(tuple.ApplicationConfigPath))
                    ApplicationConfig.WriteConfiguration(tuple.ApplicationConfigPath, tuple.ApplicationConfig);

                // Restore old monitor status.
                ViewModel.MainPageViewModel.MonitorNewApplications = oldMonitorNewApplications;
            }
        }

        /* Set default index on entry. */
        private void SetDefaultSelectionIndex()
        {
            ViewModel.SelectedIndex = 0;
        }

        private void Image_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_setApplicationImageCommand.CanExecute(null))
            {
                _setApplicationImageCommand.Execute(null);
                ViewModel.RaiseApplicationChangedEvent();
                e.Handled = true;
            }
        }

        private void CreateShortcut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Implement CreateShortcut
        }
    }
}
