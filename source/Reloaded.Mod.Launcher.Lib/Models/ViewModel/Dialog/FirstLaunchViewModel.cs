namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel used to display the First Launch Window
/// </summary>
public class FirstLaunchViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// The current page of the first launch Window to display.
    /// </summary>
    public FirstLaunchPage FirstLaunchPage { get; set; } = FirstLaunchPage.AddApplication;

    private Action? _close;

    /// <inheritdoc />
    public FirstLaunchViewModel()
    {
        IoC.RebindToConstant(this);
    }

    /// <summary>
    /// Disposes resources of this window.
    /// </summary>
    public void Dispose() => IoC.Unbind(this);

    /// <summary>
    /// Initializes the first launch ViewModel.
    /// </summary>
    /// <param name="close">Code to close the main game window.</param>
    public void Initialize(Action close)
    {
        _close = close;
    }
    
    /// <summary>
    /// Closes the First Launch Window.
    /// </summary>
    public void Close() => _close?.Invoke();

    /// <summary>
    /// Advances the First Launch window to the next page.
    /// </summary>
    public void GoToNextStep() => FirstLaunchPage += 1;

    /// <summary>
    /// Advances the First Launch window to the previous page.
    /// </summary>
    public void GoToLastStep() => FirstLaunchPage -= 1;
}