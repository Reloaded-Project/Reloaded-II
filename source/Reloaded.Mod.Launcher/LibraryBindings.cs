using IconConverter = Reloaded.Mod.Launcher.Interop.IconConverter;
using Window = System.Windows.Window;

namespace Reloaded.Mod.Launcher;

public static class LibraryBindings
{
    public static void Init(IResourceFileSelector? languageSelector, IResourceFileSelector? themeSelector)
    {
        Lib.Lib.Init
        (
            provider: new XamlResourceProvider(),
            displayMessageBox: DisplayMessage,
            context: SynchronizationContext.Current!,
            iconConverter: IconConverter.Instance,
            languageSelector: languageSelector,
            themeSelector: themeSelector,
            showModLoadSelectDialog: ShowModLoadSelectDialog, 
            displayResourceMessageBoxOkCancel: DisplayResourceMessageBoxOkCancel, 
            createResourceFileSelector: CreateResourceFileSelector, 
            showModLoaderUpdate: ShowModLoaderUpdate, 
            showModUpdate: ShowModUpdate,
            configureNuGetFeeds: ConfigureNuGetFeeds,
            configureModDialog: ConfigureModDialog,
            showMissingCoreDependency: ShowMissingCoreDependency,
            editModDialog: EditModDialog,
            publishModDialog: PublishModDialog,
            showEditModUserConfig: ShowEditModUserConfig,
            showFetchPackageDialog: ShowFetchPackageDialog,
            showSelectAddedGameDialog: ShowSelectAddedGameDialog,
            showAddAppMismatchDialog: ShowAddAppMismatchDialog,
            showApplicationWarningDialog: ShowApplicationWarningDialog,
            showRunAppViaWineDialog: ShowRunAppViaWineDialog,
            showEditPackDialog: ShowEditPackDialog,
            showInstallModPackDialog: ShowInstallModPackDialog,
            initControllerSupport: ControllerSupport.Init
        );
    }

    private static IResourceFileSelector CreateResourceFileSelector(string directoryPath)
    {
        return new XamlFileSelector(directoryPath);
    }

    private static bool DisplayMessage(string title, string message, Actions.DisplayMessageBoxParams parameters)
    {
        ReloadedWindow window = parameters.Type switch
        {
            Actions.MessageBoxType.Ok => new Pages.Dialogs.MessageBox(title, message),
            Actions.MessageBoxType.OkCancel => new MessageBoxOkCancel(title, message),
            _ => throw new ArgumentOutOfRangeException()
        };

        window.WindowStartupLocation = (WindowStartupLocation)parameters.StartupLocation;
        if (parameters.Width != default)
        {
            window.SizeToContent = SizeToContent.Height;
            window.Width = parameters.Width;
        }

        if (parameters.Timeout != default)
        {
            // Close window after timeout if open.
            Task.Delay(parameters.Timeout).ContinueWith(o =>
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                {
                    if (Application.Current.Windows.Cast<Window>().Any(x => x == window))
                        window.Close();
                });
            });
        }

        var result = window.ShowDialog();
        return result.HasValue && result.Value;
    }

    private static bool DisplayResourceMessageBoxOkCancel(string title, string message, string ok, string cancel)
    {
        var wineDialog = new XamlResourceMessageBoxOkCancel(title, message, ok, cancel);
        wineDialog.MaxWidth = 680;
        return ShowDialogAndGetResult(wineDialog);
    }
    private static bool ShowModLoadSelectDialog(LoadModSelectDialogViewModel viewmodel)
    {
        var loadModSelectDialog = new LoadModSelectDialog(viewmodel);
        loadModSelectDialog.Owner = Application.Current.MainWindow;
        
        return ShowDialogAndGetResult(loadModSelectDialog);
    }

    private static bool EditModDialog(EditModDialogViewModel viewmodel, object? owner)
    {
        var createModDialog = new EditModDialog(viewmodel);
        if (owner != null)
            createModDialog.Owner = Window.GetWindow((DependencyObject)owner);
        
        return ShowDialogAndGetResult(createModDialog);
    }

    private static bool ShowMissingCoreDependency(MissingCoreDependencyDialogViewModel viewmodel) => ShowDialogAndGetResult(new MissingCoreDependencyDialog(viewmodel));
    private static bool ConfigureModDialog(ConfigureModDialogViewModel viewmodel) => ShowDialogAndGetResult(new ConfigureModDialog(viewmodel));
    private static bool ShowModLoaderUpdate(ModLoaderUpdateDialogViewModel viewmodel) => ShowDialogAndGetResult(new ModLoaderUpdateDialog(viewmodel));
    private static bool ShowModUpdate(ModUpdateDialogViewModel viewmodel) => ShowDialogAndGetResult(new ModUpdateDialog(viewmodel));
    private static bool ConfigureNuGetFeeds(ConfigureNuGetFeedsDialogViewModel viewmodel) => ShowDialogAndGetResult(new ConfigureNuGetFeedsDialog(viewmodel));
    private static bool ShowEditModUserConfig(EditModUserConfigDialogViewModel viewmodel) => ShowDialogAndGetResult(new EditModUserConfigDialog(viewmodel));
    private static bool PublishModDialog(PublishModDialogViewModel viewmodel) => ShowDialogAndGetResult(new PublishModDialog(viewmodel));
    private static bool ShowFetchPackageDialog(DownloadPackageViewModel viewmodel) => ShowDialogAndGetResult(new DownloadPackageDialog(viewmodel));
    private static bool ShowAddAppMismatchDialog(AddAppHashMismatchDialogViewModel viewmodel) => ShowDialogAndGetResult(new AddAppHashMismatchDialog(viewmodel));
    private static bool ShowApplicationWarningDialog(AddApplicationWarningDialogViewModel viewmodel) => ShowDialogAndGetResult(new ShowApplicationWarningDialog(viewmodel));
    private static bool ShowInstallModPackDialog(InstallModPackDialogViewModel viewmodel) => ShowDialogAndGetResult(new InstallModPackDialog(viewmodel));
    private static bool ShowRunAppViaWineDialog() => ShowDialogAndGetResult(new RunAppViaWineDialog());

    private static bool ShowEditPackDialog(EditModPackDialogViewModel viewmodel) => ShowDialogAndGetResult(new EditModPackDialog(viewmodel));

    private static IndexAppEntry? ShowSelectAddedGameDialog(SelectAddedGameDialogViewModel viewmodel)
    {
        var result = ShowDialogAndGetResult(new SelectAddedGameDialog(viewmodel));
        return result ? viewmodel.SelectedEntry : null;
    }

    private static bool ShowDialogAndGetResult(this Window window)
    {
        var result = window.ShowDialog();
        return result.HasValue && result.Value;
    }
}