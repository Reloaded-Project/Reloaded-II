using System.Security.Principal;
using System.Threading;
using Reloaded.Mod.Launcher.Lib.Interop;
using Reloaded.Mod.Launcher.Lib.Static;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Used for performing operations that affect the whole launcher library.
/// </summary>
public static class Lib
{
    /// <summary>
    /// Returns if the application is running as root/sudo/administrator.
    /// </summary>
    public static bool IsElevated { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    /// <summary>
    /// Used for converting icons, image formats to `.ico`.
    /// </summary>
    public static IIconConverter IconConverter { get; private set; } = null!;

    /// <summary>
    /// Used to select a language for the application.
    /// </summary>
    public static IResourceFileSelector? LanguageSelector { get; set; } = null!;

    /// <summary>
    /// Used to select a theme for the application.
    /// </summary>
    public static IResourceFileSelector? ThemeSelector { get; set; } = null!;

    /// <summary>
    /// Initializes the library components.
    /// </summary>
    /// <param name="iconConverter">Used for converting icons.</param>
    /// <param name="themeSelector">Allows you to select a theme for the application.</param>
    /// <param name="createResourceFileSelector">Creates a resource file selector.</param>
    /// <param name="displayMessageBox">Delegate used to display a message box to the screen.</param>
    /// <param name="provider">Provider for the resources contained within.</param>
    /// <param name="context">The synchronization context to run library components by default on.</param>
    /// <param name="downloadModArchives">Shows a dialog for downloading mod archives.</param>
    /// <param name="displayResourceMessageBoxOkCancel">Displays a message box with 'ok' and 'cancel', strings fetched from resources.</param>
    /// <param name="showModLoadSelectDialog">Shows a dialog to load a mod into a Reloaded process.</param>
    /// <param name="showModLoaderUpdate">Shows the dialog to perform a mod loader update.</param>
    /// <param name="showModUpdate">Shows the dialog to perform a mod update.</param>
    /// <param name="showNuGetFetchPackage">Shows the dialog to fetch NuGet packages.</param>
    /// <param name="configureNuGetFeeds">Allows the user to configure all NuGet feeds.</param>
    /// <param name="configureModDialog">Allows the user to configure an individual managed mod.</param>
    /// <param name="showMissingCoreDependency">Provides a dialog that allows the user to show all missing .NET Core dependencies.</param>
    /// <param name="editModDialog">Provides a dialog allowing for editing of an individual mod configuration.</param>
    /// <param name="languageSelector">Allows you to select a language for the application.</param>
    /// <param name="publishModDialog">Shows a dialog that can be used to publish an individual mod.</param>
    /// <param name="showEditModUserConfig">Shows a dialog to edit an individual user configuration.</param>
    public static void Init(IDictionaryResourceProvider provider, SynchronizationContext context, IIconConverter iconConverter, 
        IResourceFileSelector? languageSelector, IResourceFileSelector? themeSelector,
        Actions.CreateResourceFileSelectorDelegate createResourceFileSelector,
        Actions.DisplayMessageboxDelegate displayMessageBox, 
        Actions.DownloadModArchivesDelegate downloadModArchives,
        Actions.DisplayResourceMessageBoxOkCancelDelegate displayResourceMessageBoxOkCancel,
        Actions.ShowModLoadSelectDialogDelegate showModLoadSelectDialog,
        Actions.ShowModLoaderUpdateDialogDelegate showModLoaderUpdate, Actions.ShowModUpdateDialogDelegate showModUpdate, 
        Actions.ShowNugetFetchPackageDialogDelegate showNuGetFetchPackage, Actions.ConfigureNuGetFeedsDialogDelegate configureNuGetFeeds,
        Actions.ConfigureModDialogDelegate configureModDialog, Actions.ShowMissingCoreDependencyDialogDelegate showMissingCoreDependency,
        Actions.EditModDialogDelegate editModDialog, Actions.PublishModDialogDelegate publishModDialog,
        Actions.ShowEditModUserConfigDialogDelegate showEditModUserConfig)
    {
        Resources.Init(provider);
        IconConverter = iconConverter;
        LanguageSelector = languageSelector;
        ThemeSelector = themeSelector;
        Actions.ShowModLoadSelectDialog = showModLoadSelectDialog;
        Actions.DownloadModArchives = downloadModArchives;
        Actions.DisplayResourceMessageBoxOkCancel = displayResourceMessageBoxOkCancel;
        Actions.CreateResourceFileSelector = createResourceFileSelector;
        Actions.DisplayMessagebox = displayMessageBox;
        Actions.SynchronizationContext = context;
        Actions.ShowModLoaderUpdateDialog = showModLoaderUpdate;
        Actions.ShowModUpdateDialog = showModUpdate;
        Actions.ShowNugetFetchPackageDialog = showNuGetFetchPackage;
        Actions.ConfigureNuGetFeeds = configureNuGetFeeds;
        Actions.ConfigureModDialog = configureModDialog;
        Actions.ShowMissingCoreDependencyDialog = showMissingCoreDependency;
        Actions.EditModDialog = editModDialog;
        Actions.PublishModDialog = publishModDialog;
        Actions.ShowEditModUserConfig = showEditModUserConfig;
    }
}