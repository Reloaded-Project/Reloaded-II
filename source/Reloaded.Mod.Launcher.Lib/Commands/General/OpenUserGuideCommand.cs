namespace Reloaded.Mod.Launcher.Lib.Commands.General;

/// <summary>
/// Command that opens the user guide website.
/// </summary>
public class OpenUserGuideCommand : ICommand
{
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter) => ProcessExtensions.OpenFileWithDefaultProgram("https://github.com/Reloaded-Project/Reloaded-II#reloaded-for-end-users");

#pragma warning disable 67
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67
}