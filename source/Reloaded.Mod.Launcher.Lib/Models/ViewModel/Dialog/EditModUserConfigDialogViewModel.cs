namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for editing the user configuration of a mod.
/// </summary>
public class EditModUserConfigDialogViewModel
{
    /// <summary>
    /// The configuration to be edited.
    /// </summary>
    public ModUserConfig Config { get; set; }

    private readonly PathTuple<ModUserConfig> _pathTuple;

    /// <summary/>
    public EditModUserConfigDialogViewModel(PathTuple<ModUserConfig> modTuple)
    {
        _pathTuple = modTuple;
        Config = modTuple.Config;
    }

    /// <summary>
    /// Asynchronously saves the user configuration for the mod.
    /// </summary>
    public async Task SaveAsync() => await _pathTuple.SaveAsync();
}