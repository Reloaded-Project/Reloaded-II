using System;
using System.Threading;
using System.Windows;
using Reloaded.Mod.Launcher.Interop;
using Reloaded.Mod.Launcher.Lib.Interop;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Dialogs;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.Theme.Default;

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
            downloadModArchives: DownloadModArchives, 
            displayResourceMessageBoxOkCancel: DisplayResourceMessageBoxOkCancel, 
            createResourceFileSelector: CreateResourceFileSelector, 
            showModLoaderUpdate: ShowModLoaderUpdate, 
            showModUpdate: ShowModUpdate,
            showNuGetFetchPackage: ShowNuGetFetchPackage,
            configureNuGetFeeds: ConfigureNuGetFeeds,
            configureModDialog: ConfigureModDialog,
            showMissingCoreDependency: ShowMissingCoreDependency,
            editModDialog: EditModDialog,
            publishModDialog: PublishModDialog,
            showEditModUserConfig: ShowEditModUserConfig
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
    private static bool ShowNuGetFetchPackage(NugetFetchPackageDialogViewModel viewmodel) => ShowDialogAndGetResult(new NugetFetchPackageDialog(viewmodel));
    private static bool ShowModLoaderUpdate(ModLoaderUpdateDialogViewModel viewmodel) => ShowDialogAndGetResult(new ModLoaderUpdateDialog(viewmodel));
    private static bool ShowModUpdate(ModUpdateDialogViewModel viewmodel) => ShowDialogAndGetResult(new ModUpdateDialog(viewmodel));
    private static bool ConfigureNuGetFeeds(ConfigureNuGetFeedsDialogViewModel viewmodel) => ShowDialogAndGetResult(new ConfigureNuGetFeedsDialog(viewmodel));
    private static bool DownloadModArchives(DownloadModArchiveViewModel viewmodel) => ShowDialogAndGetResult(new DownloadModArchiveDialog(viewmodel));
    private static bool ShowEditModUserConfig(EditModUserConfigDialogViewModel viewmodel) => ShowDialogAndGetResult(new EditModUserConfigDialog(viewmodel));
    private static bool PublishModDialog(PublishModDialogViewModel viewmodel) => ShowDialogAndGetResult(new PublishModDialog(viewmodel));

    private static bool ShowDialogAndGetResult(this Window window)
    {
        var result = window.ShowDialog();
        return result.HasValue && result.Value;
    }
}