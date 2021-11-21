using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs.FirstLaunch;

public class CompleteViewModel : ObservableObject
{
    private readonly FirstLaunchViewModel _firstLaunchViewModel;

    public CompleteViewModel(FirstLaunchViewModel firstLaunchViewModel)
    {
        _firstLaunchViewModel = firstLaunchViewModel;
    }

    public void Close()
    {
        _firstLaunchViewModel.Close();
    }

    public void OpenDocumentation()
    {
        new OpenDocumentationCommand().Execute(null);
    }

    public void OpenUserGuide()
    {
        new OpenUserGuideCommand().Execute(null);
    }

    public void GoToPreviousPage()
    {
        _firstLaunchViewModel.GoToLastStep();
    }
}