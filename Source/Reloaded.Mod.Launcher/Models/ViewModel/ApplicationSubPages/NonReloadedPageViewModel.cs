using System.Diagnostics;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class NonReloadedPageViewModel : ObservableObject
    {
        public ApplicationViewModel ApplicationViewModel { get; set; }

        public NonReloadedPageViewModel(ApplicationViewModel appViewModel)
        {
            ApplicationViewModel = appViewModel;
        }
    }
}
