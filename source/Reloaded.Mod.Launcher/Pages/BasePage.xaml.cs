using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class BasePage : ReloadedIIPage
    {
        public MainPageViewModel ViewModel { get; set; }

        public BasePage() : base()
        {
            InitializeComponent();
            ViewModel = IoC.Get<MainPageViewModel>();
            this.AnimateInFinished += OnAnimateInFinished;
        }

        /* Preconfigured Buttons */
        private void AddApp_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel.AddApplicationCommand.Execute(null);
        }

        private void ManageMods_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel.Page = BaseSubPage.ManageMods;
        }

        private void LoaderSettings_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel.Page = BaseSubPage.SettingsPage;
        }

        private void DownloadMods_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Page = BaseSubPage.DownloadMods;
        }

        private void Application_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prepare for parameter transfer.
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is ImageApplicationPathTuple tuple)
                {
                    ViewModel.SwitchToApplication(tuple);
                }
            }
        }

    }
}
