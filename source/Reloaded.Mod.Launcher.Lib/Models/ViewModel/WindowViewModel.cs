namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// View Model for the main application window.
/// </summary>
public class WindowViewModel : ObservableObject
{
    /// <summary>
    /// The currently displayed page on this window.
    /// </summary>
    public PageBase CurrentPage
    {
        get;
        set;
    } = PageBase.Splash;

    /// <summary>
    /// The title of the main window.
    /// </summary>
    public string WindowTitle
    {
        get;
        set;
    } = "Reloaded II";
}