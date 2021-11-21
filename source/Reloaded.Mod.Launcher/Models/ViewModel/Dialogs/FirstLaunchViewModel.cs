using System;
using Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;

public class FirstLaunchViewModel : ObservableObject
{
    public FirstLaunchPage FirstLaunchPage { get; set; } = FirstLaunchPage.AddApplication;

    private Action _close;

    public FirstLaunchViewModel()
    {
        IoC.Kernel.Rebind<FirstLaunchViewModel>().ToConstant(this);
    }

    public void Initialize(Action close)
    {
        _close = close;
    }
    
    public void Close()
    {
        _close?.Invoke();
    }

    public void GoToNextStep()
    {
        FirstLaunchPage += 1;
    }

    public void GoToLastStep()
    {
        FirstLaunchPage -= 1;
    }
}