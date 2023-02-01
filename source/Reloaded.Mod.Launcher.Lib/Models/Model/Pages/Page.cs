namespace Reloaded.Mod.Launcher.Lib.Models.Model.Pages;

/// <summary>
/// Represents an individual launcher page that the user can browse to.
/// </summary>
public enum Page
{
    /// <summary>
    /// Page allowing the user to download individual mods.
    /// </summary>
    DownloadMods,

    /// <summary>
    /// Browse to the Manage Mods page, where the user. 
    /// </summary>
    ManageMods,

    /// <summary>
    /// Settings page, which also contains information about the Mod Loader itself.
    /// </summary>
    SettingsPage,

    /// <summary>
    /// Page for an individual application.
    /// </summary>
    Application,
}