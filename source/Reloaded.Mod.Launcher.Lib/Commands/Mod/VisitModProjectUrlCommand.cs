namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// A command that allows you to visit a website for a given mod.
/// </summary>
public class VisitModProjectUrlCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModConfig>? _modTuple;

    /// <inheritdoc />
    public VisitModProjectUrlCommand(PathTuple<ModConfig>? modTuple)
    {
        _modTuple = modTuple;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (_modTuple == null)
            return false;

        return !string.IsNullOrEmpty(_modTuple.Config.ProjectUrl);
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        ProcessExtensions.OpenHyperlink(_modTuple!.Config.ProjectUrl);
    }
}