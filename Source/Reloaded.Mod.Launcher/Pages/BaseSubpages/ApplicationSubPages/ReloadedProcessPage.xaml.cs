using System;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages
{
    /// <summary>
    /// Interaction logic for RunningProcessPage.xaml
    /// </summary>
    public partial class ReloadedProcessPage : ApplicationSubPage
    {
        public ReloadedApplicationViewModel ViewModel { get; set; }

        public ReloadedProcessPage(ApplicationViewModel model)
        {
            InitializeComponent();
            ViewModel = new ReloadedApplicationViewModel(model);
            this.AnimateOutStarted += Dispose;
        }

        private void Dispose()
        {
            ViewModel.CancelToken();
        }
    }
}
