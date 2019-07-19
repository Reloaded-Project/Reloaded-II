using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages
{
    /// <summary>
    /// Interaction logic for NonReloadedProcessPage.xaml
    /// </summary>
    public partial class NonReloadedProcessPage : ApplicationSubPage
    {
        public NonReloadedPageViewModel ViewModel { get; set; }

        public NonReloadedProcessPage()
        {
            InitializeComponent();
            ViewModel = IoC.Get<NonReloadedPageViewModel>();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var process = ViewModel.ApplicationViewModel.SelectedProcess;
            if (!process.HasExited)
            {
                var injector = new ApplicationInjector(ViewModel.ApplicationViewModel.SelectedProcess);
                injector.Inject();

                // Exit page.
                ViewModel.ApplicationViewModel.Page = Enum.ApplicationSubPage.ReloadedProcess;
            }
        }
    }
}
