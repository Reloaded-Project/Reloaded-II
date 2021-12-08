using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Static;

namespace Reloaded.Mod.Launcher.Lib.Commands.Download;

/// <summary>
/// This command can be used to check updates and dependencies for all mods.
/// </summary>
public class CheckForUpdatesAndDependenciesCommand : WithCanExecuteChanged, ICommand
{
    private bool _canExecute = true;

    /* ICommand. */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return _canExecute;
    }

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        _canExecute = false;
        RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        var updates      = await Task.Run(Update.CheckForModUpdatesAsync);
        var dependencies = Update.CheckMissingDependencies(out var missingDependencies);

        if ((!updates) && (!dependencies))
        {
            Actions.DisplayMessagebox?.Invoke(Resources.NoUpdateDialogTitle.Get(), Resources.NoUpdateDialogMessage.Get(), new Actions.DisplayMessageBoxParams()
            {
                StartupLocation = Actions.WindowStartupLocation.CenterScreen
            });
        }
        else if (dependencies)
        {
            try
            {
                await Update.DownloadNuGetPackagesAsync(missingDependencies, false, false);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        _canExecute = true;
        RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}