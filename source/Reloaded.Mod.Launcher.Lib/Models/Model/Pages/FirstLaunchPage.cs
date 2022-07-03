namespace Reloaded.Mod.Launcher.Lib.Models.Model.Pages;

/// <summary>
/// Contains the list of pages accessible from the first launch window.
/// </summary>
public enum FirstLaunchPage
{
    /// <summary>
    /// Page: Shows user how to add Application.
    /// </summary>
    AddApplication,

    /// <summary>
    /// Page: Shows user how to extract a Modification.
    /// </summary>
    AddModExtract,

    /// <summary>
    /// Page: Shows user how to configure a Mod.
    /// </summary>
    ModConfigPage,

    /// <summary>
    /// Page: Shows user how to enable a Mod.
    /// </summary>
    ModEnablePage,

    /// <summary>
    /// Page: Shows user some useful links and a way to close the window.
    /// </summary>
    Complete,
}