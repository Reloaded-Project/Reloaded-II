using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for DownloadModsPage.xaml
    /// </summary>
    public partial class DownloadModsPage : ReloadedIIPage
    {
        public DownloadModsViewModel ViewModel { get; set; }

        public DownloadModsPage()
        {
            InitializeComponent();
            ViewModel = IoC.GetConstant<DownloadModsViewModel>();
        }
    }
}
