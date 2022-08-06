namespace Reloaded.Mod.Launcher.Lib.Commands.General;

/// <summary>
/// A command whose sole purpose is to relay its functionality 
/// to other objects by invoking delegates. 
/// The default return value for the CanExecute method is 'true'.
/// <see cref="CanExecute"/> is expected to return a different value.
/// </summary>
public class RelayCommand : ICommand
{
    /// <summary>
    /// Executed after the execute method is ran.
    /// </summary>
    public event Action<object?>? AfterExecute; 

    #region Private members
    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    private readonly Action<object?>? _execute;

    /// <summary>
    /// True if command is executing, false otherwise
    /// </summary>
    private readonly Predicate<object?>? _canExecute;
    #endregion

    /// <summary>
    /// Initializes a new instance of <see cref="RelayCommand"/> that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action<object?>? execute) : this(execute, canExecute: null) { }

    /// <summary>
    /// Initializes a new instance of <see cref="RelayCommand"/>.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<object?>? execute, Predicate<object?>? canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException("execute");
        _canExecute = canExecute;
    }

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// <returns>True if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Executes the <see cref="RelayCommand"/> on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    public void Execute(object? parameter)
    {
        _execute!(parameter);
        AfterExecute?.Invoke(parameter);
    }
}