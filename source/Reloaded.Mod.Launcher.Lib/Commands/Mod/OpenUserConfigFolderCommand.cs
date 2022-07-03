namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command allowing you to open the user config folder for an individual mod.
/// </summary>
public class OpenUserConfigFolderCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModUserConfig>? _userConfigTuple;

    /// <inheritdoc />
    public OpenUserConfigFolderCommand(PathTuple<ModUserConfig>? userConfigTuple)
    {
        _userConfigTuple = userConfigTuple;
    }

    /* ICommand */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (_userConfigTuple == null)
            return false;

        string directoryPath = Path.GetDirectoryName(_userConfigTuple.Path)!;
        return Directory.Exists(directoryPath);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void Execute(object? parameter)
    {
        string directoryPath = Path.GetDirectoryName(_userConfigTuple!.Path)!;
        ProcessExtensions.OpenFileWithExplorer(directoryPath!);
    }
}