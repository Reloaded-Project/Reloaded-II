namespace Reloaded.Mod.Launcher.Lib.Commands.Templates;

/// <summary>
/// Represents a command that allows you to inject custom logic after an execution
/// of an existing command.
/// </summary>
public class CallbackCommand : ICommand
{
    private Action? _afterExecute;
    private ICommand _internal;

    /// <summary/>
    /// <param name="internalCommand"></param>
    /// <param name="afterExecute">Called af</param>
    public CallbackCommand(ICommand internalCommand, Action? afterExecute)
    {
        _internal = internalCommand;
        _internal.CanExecuteChanged += (sender, args) => CanExecuteChanged?.Invoke(sender, args);
        _afterExecute = afterExecute;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _internal.CanExecute(parameter);

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        _internal.Execute(parameter);
        _afterExecute?.Invoke();
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
}