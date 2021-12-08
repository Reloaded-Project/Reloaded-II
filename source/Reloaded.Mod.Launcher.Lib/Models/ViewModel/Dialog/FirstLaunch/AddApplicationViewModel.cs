using Reloaded.Mod.Launcher.Lib.Commands.Application;
using Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog.FirstLaunch;

/// <summary>
/// ViewModel for the application adding component of the Reloaded tutorial.
/// </summary>
public class AddApplicationViewModel : ObservableObject
{
    /// <summary>
    /// Provides access to the common shared First launch state.
    /// </summary>
    public FirstLaunchViewModel FirstLaunchViewModel { get; set; }

    /// <summary/>
    public AddApplicationCommand AddApplicationCommand { get; set; }

    /// <inheritdoc />
    public AddApplicationViewModel(FirstLaunchViewModel firstLaunchViewModel, AddApplicationCommand addApplicationCommand)
    {
        FirstLaunchViewModel = firstLaunchViewModel;
        AddApplicationCommand = addApplicationCommand;
    }
}