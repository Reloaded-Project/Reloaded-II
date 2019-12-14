using System;
using System.Diagnostics;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class NonReloadedPageViewModel : ObservableObject
    {
        public ApplicationViewModel ApplicationViewModel { get; set; }

        public NonReloadedPageViewModel(ApplicationViewModel appViewModel)
        {
            ApplicationViewModel = appViewModel;
            ApplicationViewModel.SelectedProcess.EnableRaisingEvents = true;
            ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;
        }

        private void SelectedProcessOnExited(object sender, EventArgs e) => ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
    }
}
