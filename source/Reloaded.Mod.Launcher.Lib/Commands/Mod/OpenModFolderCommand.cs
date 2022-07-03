namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command allowing you to open the folder where an individual mod is contained.
/// </summary>
public class OpenModFolderCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModConfig>? _modTuple;

    /// <inheritdoc />
    public OpenModFolderCommand(PathTuple<ModConfig>? modTuple)
    {
        _modTuple = modTuple;
    }
        
    /* ICommand */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (_modTuple == null) 
            return false;

        string directoryPath = Path.GetDirectoryName(_modTuple.Path)!;
        return Directory.Exists(directoryPath);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void Execute(object? parameter)
    {
        string directoryPath = Path.GetDirectoryName(_modTuple!.Path)!;
        ProcessExtensions.OpenFileWithExplorer(directoryPath!);
    }
}