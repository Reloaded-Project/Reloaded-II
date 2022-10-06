namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel responsible for selecting a mod to load from the Reloaded Process application page.
/// </summary>
public class SelectPackModDialogViewModel
{
    /// <summary>
    /// All mods selectable in this viewmodel.
    /// </summary>
    public List<PathTuple<ModConfig>> Mods { get; set; }
    
    /// <summary>
    /// The currently selected mod.
    /// </summary>
    public PathTuple<ModConfig>? SelectedMod { get; set; }

    /// <summary/>
    public SelectPackModDialogViewModel(List<PathTuple<ModConfig>> mods)
    {
        Mods = mods;
    }
}