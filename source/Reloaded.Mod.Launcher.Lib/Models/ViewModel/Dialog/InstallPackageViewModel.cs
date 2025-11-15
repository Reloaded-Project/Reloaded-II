namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for downloading an individual package.
/// </summary>
public class InstallPackageViewModel : ObservableObject
{
    /// <summary>
    /// The display text for the package installation.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// The title for the package installation.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// The current progress of the installation operation.
    /// Range 0-100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// True if the installation is complete, else false.
    /// </summary>
    public bool IsComplete { get; set; }
}