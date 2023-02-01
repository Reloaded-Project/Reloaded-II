namespace Reloaded.Mod.Launcher.Lib.Models.Model.Pages;

/// <summary>
/// Represents an individual page in the <see cref="Page.Application"/> submenu.
/// </summary>
public enum ApplicationSubPage
{
    /// <summary>
    /// Default Value. None.
    /// </summary>
    Null,

    /// <summary>
    /// Displays details about a process where the loader is not loaded.
    /// </summary>
    NonReloadedProcess,

    /// <summary>
    /// Displays details about a process where the loader has been loaded.
    /// </summary>
    ReloadedProcess,

    /// <summary>
    /// Displays and allows the user to configure all modifications.
    /// </summary>
    ApplicationSummary,

    /// <summary>
    /// Allows the user to edit the details of the application.
    /// </summary>
    EditApplication
}