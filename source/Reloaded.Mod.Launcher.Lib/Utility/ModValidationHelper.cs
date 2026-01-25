namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Helper class for validating mod configurations after installation.
/// </summary>
public static class ModValidationHelper
{
    /// <summary>
    /// Validates that a newly installed mod has compatible applications configured.
    /// If no compatible apps are found, prompts the user to manually select one.
    /// </summary>
    /// <param name="mod">The mod configuration to validate.</param>
    /// <param name="applicationConfigService">Service providing application configurations.</param>
    /// <param name="modConfigService">Service providing mod configurations.</param>
    /// <param name="ownerWindow">Optional owner window for the edit dialog.</param>
    public static void ValidateModAppCompatibility(
        PathTuple<ModConfig> mod,
        ApplicationConfigService applicationConfigService,
        ModConfigService modConfigService,
        object? ownerWindow = null)
    {
        if (mod.Config.IsUniversalMod)
            return;

        if (mod.Config.SupportedAppId.Length > 0)
        {
            var match = applicationConfigService.Items.FirstOrDefault(
                app => mod.Config.SupportedAppId.Contains(app.Config.AppId));

            if (match == null)
            {
                PromptAndShowEditDialog(
                    Resources.NoCompatibleAppsInConfigTitle.Get(),
                    Resources.NoCompatibleAppsInConfigDescription.Get(),
                    mod, applicationConfigService, modConfigService, ownerWindow);
            }
        }
        else
        {
            PromptAndShowEditDialog(
                Resources.NoAppsInConfigTitle.Get(),
                Resources.NoAppsInConfigDescription.Get(),
                mod, applicationConfigService, modConfigService, ownerWindow);
        }
    }

    private static void PromptAndShowEditDialog(
        string title,
        string description,
        PathTuple<ModConfig> mod,
        ApplicationConfigService applicationConfigService,
        ModConfigService modConfigService,
        object? ownerWindow)
    {
        var message = $"{description}\n{Resources.AppSelectionQuestion.Get()}";
        bool loadAppPage = Actions.DisplayResourceMessageBoxOkCancel!(
            title, message, Resources.Yes.Get(), Resources.No.Get());

        if (loadAppPage)
        {
            var viewmodel = new EditModDialogViewModel(mod, applicationConfigService, modConfigService);
            viewmodel.Page = EditModPage.Special;
            Actions.EditModDialog(viewmodel, ownerWindow);
        }
    }
}
