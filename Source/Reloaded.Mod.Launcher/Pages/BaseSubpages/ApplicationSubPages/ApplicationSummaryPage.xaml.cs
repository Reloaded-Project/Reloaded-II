using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages
{
    /// <summary>
    /// Interaction logic for ApplicationSummaryPage.xaml
    /// </summary>
    public partial class ApplicationSummaryPage : ApplicationSubPage
    {
        public ApplicationSummaryViewModel ViewModel { get; set; }

        public ApplicationSummaryPage()
        {
            InitializeComponent();
            ViewModel = IoC.Get<ApplicationSummaryViewModel>();
        }
    }
}
