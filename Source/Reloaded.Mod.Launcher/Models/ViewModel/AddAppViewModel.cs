using Reloaded.Mod.Interfaces;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class AddAppViewModel : ObservableObject
    {
        public IApplicationConfig Application { get; set; }

        public MainPageViewModel MainPageViewModel { get; set; }
        public int SelectedIndex { get; set; } = 0;

        public AddAppViewModel(MainPageViewModel viewModel)
        {
            MainPageViewModel = viewModel;
        }
    }
}
