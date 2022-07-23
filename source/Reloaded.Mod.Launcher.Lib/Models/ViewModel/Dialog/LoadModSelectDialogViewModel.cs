namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel responsible for selecting a mod to load from the Reloaded Process application page.
/// </summary>
public class LoadModSelectDialogViewModel
{
    /// <summary/>
    public ApplicationViewModel ApplicationViewModel { get; set; }

    /// <summary/>
    public ReloadedAppViewModel ReloadedAppViewModel { get; set; }

    /// <summary>
    /// The currently selected modification to be loaded.
    /// </summary>
    public PathTuple<ModConfig>? SelectedMod { get; set; }

    /// <summary/>
    public LoadModSelectDialogViewModel(ApplicationViewModel applicationViewModel, ReloadedAppViewModel reloadedAppViewModel)
    {
        ApplicationViewModel = applicationViewModel;
        ReloadedAppViewModel = reloadedAppViewModel;
    }

    /// <summary>
    /// Allows you to load an individual modification.
    /// </summary>
    public void LoadMod()
    {
        try
        {
            Task.Run(() => ReloadedAppViewModel.Client?.LoadModAsync(SelectedMod!.Config.ModId));
        }
        catch (Exception)
        {
             /* Ignored */
        }
    }
}