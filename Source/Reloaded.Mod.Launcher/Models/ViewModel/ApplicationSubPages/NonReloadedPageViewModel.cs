using System.Diagnostics;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class NonReloadedPageViewModel : ObservableObject
    {
        public Process Process { get; set; }

        public NonReloadedPageViewModel(ApplicationViewModel appViewModel)
        {
            Process = appViewModel.SelectedProcess;
        }
    }
}
