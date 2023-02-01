namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog.FirstLaunch;

/// <inheritdoc />
public class CompleteViewModel : ObservableObject
{
    private readonly FirstLaunchViewModel _firstLaunchViewModel;

    /// <inheritdoc />
    public CompleteViewModel(FirstLaunchViewModel firstLaunchViewModel)
    {
        _firstLaunchViewModel = firstLaunchViewModel;
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close()
    {
        _firstLaunchViewModel.Close();
    }

    /// <summary>
    /// Opens the Reloaded documentation.
    /// </summary>
    public void OpenDocumentation()
    {
        new OpenDocumentationCommand().Execute(null);
    }

    /// <summary>
    /// Opens the Reloaded user guide.
    /// </summary>
    public void OpenUserGuide()
    {
        new OpenUserGuideCommand().Execute(null);
    }

    /// <summary>
    /// Returns to the previous page.
    /// </summary>
    public void GoToPreviousPage()
    {
        _firstLaunchViewModel.GoToLastStep();
    }
}