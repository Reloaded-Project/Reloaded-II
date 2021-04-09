using Reloaded.Mod.Launcher.Pages.Dialogs;

namespace Reloaded.Mod.Launcher.Misc
{
    public static class CompatibilityDialogs
    {
        public static bool WineShowLaunchDialog()
        {
            var wineDialog = new XamlResourceMessageBoxOkCancel("WineCompatibilityNoticeTitle", "WineCompatibilityNoticeText", "WineCompatibilityNoticeOk", "MessageBoxButtonCancel");
            wineDialog.MaxWidth = 680;

            var result = wineDialog.ShowDialog();
            return result.HasValue && result.Value;
        }
    }
}
