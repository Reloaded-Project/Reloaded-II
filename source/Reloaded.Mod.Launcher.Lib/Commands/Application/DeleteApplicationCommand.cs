namespace Reloaded.Mod.Launcher.Lib.Commands.Application;

/// <summary>
/// Command which can be used to delete an individual application.
/// </summary>
public class DeleteApplicationCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ApplicationConfig> _applicationTuple;
    
    /// <summary>
    /// Allows you to delete an application.
    /// </summary>
    /// <param name="applicationTuple">The application to be deleted.</param>
    public DeleteApplicationCommand(PathTuple<ApplicationConfig> applicationTuple)
    {
        _applicationTuple = applicationTuple;
    }

    /* ICommand. */

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        // Delete folder.
        var directory = Path.GetDirectoryName(_applicationTuple.Path) ?? throw new InvalidOperationException(Resources.ErrorFailedToGetDirectoryOfApplication.Get());
        Directory.Delete(directory, true);
    }
}