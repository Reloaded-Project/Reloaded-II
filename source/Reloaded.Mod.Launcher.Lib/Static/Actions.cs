#pragma warning disable CS1591

namespace Reloaded.Mod.Launcher.Lib.Static;

/// <summary>
/// Contains various delegates for performing generic actions.
/// </summary>
public static class Actions
{
    /// <summary>
    /// The synchronization context to run all CanExecute commands on.
    /// </summary>
    public static SynchronizationContext SynchronizationContext { get; set; } = null!;

    /// <summary>
    /// Displays a message box to the screen.
    /// </summary>
    public static DisplayMessageboxDelegate DisplayMessagebox { get; set; } = null!;

    /// <summary>
    /// Shows a dialog allowing for the mod loader to be updated.
    /// </summary>
    public static ShowModLoaderUpdateDialogDelegate ShowModLoaderUpdateDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog allowing for the mods to be updated.
    /// </summary>
    public static ShowModUpdateDialogDelegate ShowModUpdateDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog allowing the user to configure NuGet feeds.
    /// </summary>
    public static ConfigureNuGetFeedsDialogDelegate ConfigureNuGetFeeds { get; set; } = null!;

    /// <summary>
    /// Shows a dialog allowing the user to configure mod options.
    /// </summary>
    public static ConfigureModDialogDelegate ConfigureModDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog letting know of their missing .NET dependencies.
    /// </summary>
    public static ShowMissingCoreDependencyDialogDelegate ShowMissingCoreDependencyDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog responsible for editing an individual mod.
    /// </summary>
    public static EditModDialogDelegate EditModDialog { get; set; } = null!;

    /// <summary>
    /// Creates a resource file selector.
    /// </summary>
    public static CreateResourceFileSelectorDelegate CreateResourceFileSelector { get; set; } = null!;

    /// <summary>
    /// Displays a message box with strings sourced from <see cref="Resources.ResourceProvider"/>.
    /// </summary>
    public static DisplayResourceMessageBoxOkCancelDelegate DisplayResourceMessageBoxOkCancel { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that can be used to load a mod into a Reloaded process.
    /// </summary>
    public static ShowModLoadSelectDialogDelegate ShowModLoadSelectDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that can be used to publish an individual mod.
    /// </summary>
    public static PublishModDialogDelegate PublishModDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog allowing the user to edit their individual user config.
    /// </summary>
    public static ShowEditModUserConfigDialogDelegate ShowEditModUserConfig { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that can be used to download an individual package.
    /// </summary>
    public static ShowFetchPackageDialogDelegate ShowFetchPackageDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that can be used to select the added game.
    /// </summary>
    public static ShowSelectAddedGameDialogDelegate ShowSelectAddedGameDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that shows information to the user regarding an application hash mismatch.
    /// </summary>
    public static ShowAddAppHashMismatchDialogDelegate ShowAddAppHashMismatchDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that displays information about warnings regarding to the application.
    /// </summary>
    public static ShowApplicationWarningDialogDelegate ShowApplicationWarningDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that displays information about running an application using Reloaded on Wine.
    /// </summary>
    public static ShowRunAppViaWineDialogDelegate ShowRunAppViaWineDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that allows the user to edit an individual mod pack.
    /// </summary>
    public static ShowEditPackDialogDelegate ShowEditPackDialog { get; set; } = null!;

    /// <summary>
    /// Shows a dialog that allows the user to install an individual mod pack.
    /// </summary>
    public static ShowInstallModPackDialogDelegate ShowInstallModPackDialog { get; set; } = null!;

    /// <summary>
    /// Initializes controller support.
    /// </summary>
    public static Action InitControllerSupport { get; set; } = null!;

    /// <summary>
    /// Delegate used to display a message to user's screen.
    /// </summary>
    /// <param name="title">Title of the error.</param>
    /// <param name="message">The individual error message.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <returns>True if the dialog returned a positive ok result, else false.</returns>
    public delegate bool DisplayMessageboxDelegate(string title, string message, DisplayMessageBoxParams parameters = default);

    /// <summary>
    /// Delegate used to display a message box to user's screen.
    /// Parameters are names of resources to be sourced from <see cref="Resources.ResourceProvider"/>.
    /// </summary>
    /// <param name="title">Name of resource containing title string.</param>
    /// <param name="message">Name of resource containing message string.</param>
    /// <param name="ok">Name of resource containing string for ok button.</param>
    /// <param name="cancel">Name of resource containing string for cancel button.</param>
    /// <returns>True if the dialog returned a positive ok result, else false.</returns>
    public delegate bool DisplayResourceMessageBoxOkCancelDelegate(string title, string message, string ok, string cancel);

    public struct DisplayMessageBoxParams
    {
        public WindowStartupLocation StartupLocation = WindowStartupLocation.Manual;
        public MessageBoxType Type = MessageBoxType.Ok;

        /// <summary>
        /// Time after which the message box is automatically closed
        /// </summary>
        public TimeSpan Timeout = TimeSpan.Zero;

        /// <summary>
        /// Maximum width for the message box.
        /// </summary>
        public int Width { get; set; } = default;

        public DisplayMessageBoxParams() { }
    }

    /// <summary>
    /// Details the location where a window should spawn by default.
    /// </summary>
    public enum WindowStartupLocation
    {
        /// <summary>
        /// Window can spawn anywhere.
        /// </summary>
        Manual,

        /// <summary>
        /// Window should spawn in the center of the screen.
        /// </summary>
        CenterScreen,
        
        /// <summary>
        /// Window should spawn in the center of the owner.
        /// </summary>
        CenterOwner,
    }

    /// <summary>
    /// Type of message box to spawn.
    /// </summary>
    public enum MessageBoxType
    {
        /// <summary>
        /// Box with an OK Button.
        /// </summary>
        Ok,

        /// <summary>
        /// Box with an OK and Cancel Button
        /// </summary>
        OkCancel
    }

    /// <summary>
    /// Shows the dialog responsible for allowing the user to update the mod loader.
    /// </summary>
    /// <param name="viewModel">The view model of the individual mod loader update.</param>
    public delegate bool ShowModLoaderUpdateDialogDelegate(ModLoaderUpdateDialogViewModel viewModel);

    /// <summary>
    /// Shows the dialog responsible for allowing the user to update the individual mods.
    /// </summary>
    /// <param name="viewModel">The view model of the individual mod update.</param>
    public delegate bool ShowModUpdateDialogDelegate(ModUpdateDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that allows the user to configure the NuGet feeds for this application.
    /// </summary>
    /// <param name="viewModel">The view model of the individual dialog to configure NuGet feeds.</param>
    public delegate bool ConfigureNuGetFeedsDialogDelegate(ConfigureNuGetFeedsDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that allows the user to configure individual mod settings.
    /// </summary>
    /// <param name="viewModel">The view model of the individual dialog to configure mod from.</param>
    public delegate bool ConfigureModDialogDelegate(ConfigureModDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that shows the user currently missing .NET dependencies.
    /// </summary>
    /// <param name="viewModel">The view model of the individual dependencies.</param>
    public delegate bool ShowMissingCoreDependencyDialogDelegate(MissingCoreDependencyDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that can be used to edit an individual mod.
    /// </summary>
    /// <param name="viewModel">The view model for the edit mod menu.</param>
    /// <param name="ownerWindow">The window/dialog which is the parent of the dialog to be shown.</param>
    public delegate bool EditModDialogDelegate(EditModDialogViewModel viewModel, object? ownerWindow);

    /// <summary>
    /// Creates a resource file selector.
    /// </summary>
    /// <param name="directoryPath">The directory for which to make a resource file selector.</param>
    public delegate IResourceFileSelector CreateResourceFileSelectorDelegate(string directoryPath);

    /// <summary>
    /// Shows a dialog that can be used to load a mod into a Reloaded managed process.
    /// </summary>
    /// <param name="viewModel">The viewmodel of the mod to be loaded.</param>
    public delegate bool ShowModLoadSelectDialogDelegate(LoadModSelectDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that can be used to publish an individual mod.
    /// </summary>
    /// <param name="viewModel">The viewmodel used for publishing an individual mod.</param>
    public delegate bool PublishModDialogDelegate(PublishModDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that can be used to edit the user config of an individual mod.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for editing the user config of an individual mod.</param>
    public delegate bool ShowEditModUserConfigDialogDelegate(EditModUserConfigDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that can be used to download an individual package.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for downloading the individual package.</param>
    public delegate bool ShowFetchPackageDialogDelegate(DownloadPackageViewModel viewModel);

    /// <summary>
    /// Shows a dialog that can be used to select the added game.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for selecting the individual game.</param>
    public delegate IndexAppEntry? ShowSelectAddedGameDialogDelegate(SelectAddedGameDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that displays information about the mismatched application hash.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for selecting the individual game.</param>
    public delegate bool ShowAddAppHashMismatchDialogDelegate(AddAppHashMismatchDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that displays information about warnings regarding to the application.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for showing warnings about the application.</param>
    public delegate bool ShowApplicationWarningDialogDelegate(AddApplicationWarningDialogViewModel viewModel);
    
    /// <summary>
    /// Shows a dialog that allows the user to edit an individual mod pack.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for editing the individual mod pack.</param>
    public delegate bool ShowEditPackDialogDelegate(EditModPackDialogViewModel viewModel);

    /// <summary>
    /// Shows a dialog that displays information about running an application using Reloaded on Wine.
    /// </summary>
    public delegate bool ShowRunAppViaWineDialogDelegate();

    /// <summary>
    /// Shows a dialog that allows the user to install an individual mod pack.
    /// </summary>
    /// <param name="viewModel">The ViewModel used for installing a mod pack.</param>
    public delegate bool ShowInstallModPackDialogDelegate(InstallModPackDialogViewModel viewModel);
}