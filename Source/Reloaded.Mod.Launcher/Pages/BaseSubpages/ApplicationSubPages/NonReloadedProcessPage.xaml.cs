using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

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
            // TODO: Inject Reloaded II
        }
    }
}
