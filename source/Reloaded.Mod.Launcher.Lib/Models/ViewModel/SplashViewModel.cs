namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// View model for the contents of the splash screen.
/// </summary>
public class SplashViewModel : ObservableObject
{
    /// <summary>
    /// The text to display on the splash screen.
    /// </summary>
    public string Text { get; set; } = "";
}