namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <inheritdoc />
public class CreateModPackCommand : ICommand
{
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter) => Actions.ShowEditPackDialog(IoC.Get<EditModPackDialogViewModel>());

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
}