using System.IO;
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
        private readonly AddAppViewModel _model;

        public AddAppPage() : base()
        {  
            InitializeComponent();

            // Setup ViewModel
            _model = IoC.Get<AddAppViewModel>();
            this.DataContext = _model;
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // TODO: Save items on exit page too.
            // Filter for first load.
            if (e.RemovedItems.Count > 0)
            {
                // Backup and disable monitor status.
                bool oldMonitorNewApplications = _model.MainPageViewModel.MonitorNewApplications;
                _model.MainPageViewModel.MonitorNewApplications = false;

                // Write config.
                // Without the file existence check, what can happen is we remove an application and it immediately comes back.
                var tuple = (ImageApplicationPathTuple)e.RemovedItems[0];
                if (File.Exists(tuple.ApplicationConfigPath))
                    ApplicationConfig.WriteConfiguration(tuple.ApplicationConfigPath, (ApplicationConfig)tuple.ApplicationConfig);

                // Restore old monitor status.
                _model.MainPageViewModel.MonitorNewApplications = oldMonitorNewApplications;
            }
        }
    }
}
