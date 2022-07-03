namespace Reloaded.Mod.Launcher.Lib.Commands.General;

/// <summary>
/// Command that opens the documentation website.
/// </summary>
public class OpenDocumentationCommand : ICommand
{
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter) => ProcessExtensions.OpenFileWithDefaultProgram("https://reloaded-project.github.io/Reloaded-II/");

#pragma warning disable 67
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67
}