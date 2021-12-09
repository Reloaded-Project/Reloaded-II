using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Actions = Reloaded.Mod.Launcher.Lib.Static.Actions;

namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command allowing you to edit the configuration file of an individual mod.
/// </summary>
public class EditModCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModConfig>? _modTuple;
    private object? _parent;

    /// <inheritdoc />
    public EditModCommand(PathTuple<ModConfig>? modTuple, object? parent)
    {
        _modTuple = modTuple;
        _parent = parent;
    }

    /* Interface Implementation */

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _modTuple != null;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        Actions.EditModDialog(new EditModDialogViewModel(_modTuple!, IoC.Get<ApplicationConfigService>(), IoC.Get<ModConfigService>()), _parent);
    }
}