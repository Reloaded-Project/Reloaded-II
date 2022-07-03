namespace Reloaded.Mod.Launcher.Lib.Misc;

/// <summary>
/// Various dialogs about compatibility notices for certain configurations.
/// </summary>
public static class CompatibilityDialogs
{
    /// <summary>
    /// Shows a dialog displayed when an application is to be ran from within Wine.
    /// </summary>
    /// <returns></returns>
    public static bool WineShowLaunchDialog()
    {
        return Actions.DisplayResourceMessageBoxOkCancel("WineCompatibilityNoticeTitle", "WineCompatibilityNoticeText", "WineCompatibilityNoticeOk", "MessageBoxButtonCancel");
    }
}