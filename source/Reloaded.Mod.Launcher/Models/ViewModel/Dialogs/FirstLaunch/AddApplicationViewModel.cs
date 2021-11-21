using Reloaded.Mod.Launcher.Commands.EditAppPage;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs.FirstLaunch;

public class AddApplicationViewModel : ObservableObject
{
    public FirstLaunchViewModel FirstLaunchViewModel { get; set; }

    public AddApplicationCommand AddApplicationCommand { get; set; }

    public AddApplicationViewModel(FirstLaunchViewModel firstLaunchViewModel, AddApplicationCommand addApplicationCommand)
    {
        FirstLaunchViewModel = firstLaunchViewModel;
        AddApplicationCommand = addApplicationCommand;
    }
}