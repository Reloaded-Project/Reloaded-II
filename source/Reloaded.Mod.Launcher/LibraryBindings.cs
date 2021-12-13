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
            publishModDialog: PublishModDialog
        );
    }

    private static bool PublishModDialog(PublishModDialogViewModel viewmodel)
    {
        var window = new PublishModDialog(viewmodel);
        var result = window.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static IResourceFileSelector CreateResourceFileSelector(string directoryPath)
    {
        return new XamlFileSelector(directoryPath);
    }

    private static bool ShowModLoadSelectDialog(LoadModSelectDialogViewModel viewmodel)
    {
        var loadModSelectDialog = new LoadModSelectDialog(viewmodel);
        loadModSelectDialog.Owner = Application.Current.MainWindow;
        var result = loadModSelectDialog.ShowDialog();
        return result.HasValue && result.Value;
    }

    private static bool EditModDialog(EditModDialogViewModel viewmodel, object? owner)
    {
        var createModDialog = new EditModDialog(viewmodel);
        if (owner != null)
            createModDialog.Owner = Window.GetWindow((DependencyObject) owner);

        var result = createModDialog.ShowDialog();
        return result.HasValue && result.Value;
    }

    private static bool ShowMissingCoreDependency(MissingCoreDependencyDialogViewModel viewmodel)
    {
        var window = new MissingCoreDependencyDialog(viewmodel);
        var result = window.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static bool ConfigureModDialog(ConfigureModDialogViewModel viewmodel)
    {
        var window = new ConfigureModDialog(viewmodel);
        var result = window.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static bool ShowNuGetFetchPackage(NugetFetchPackageDialogViewModel viewmodel)
    {
        var dialog = new NugetFetchPackageDialog(viewmodel);
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static bool ShowModLoaderUpdate(ModLoaderUpdateDialogViewModel viewmodel)
    {
        var dialog = new ModLoaderUpdateDialog(viewmodel);
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
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

    private static bool ShowModUpdate(ModUpdateDialogViewModel viewmodel)
    {
        var dialog = new ModUpdateDialog(viewmodel);
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static bool ConfigureNuGetFeeds(ConfigureNuGetFeedsDialogViewModel viewmodel)
    {
        var dialog = new ConfigureNuGetFeedsDialog(viewmodel);
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
    }

    private static bool DisplayResourceMessageBoxOkCancel(string title, string message, string ok, string cancel)
    {
        var wineDialog = new XamlResourceMessageBoxOkCancel(title, message, ok, cancel);
        wineDialog.MaxWidth = 680;
        var result = wineDialog.ShowDialog();
        return result.HasValue && result.Value;
    }

    private static bool DownloadModArchives(DownloadModArchiveViewModel viewmodel)
    {
        var dialog = new DownloadModArchiveDialog(viewmodel);
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
    }
}