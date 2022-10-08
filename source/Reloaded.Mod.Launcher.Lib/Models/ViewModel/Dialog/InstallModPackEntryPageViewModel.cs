using Reloaded.Mod.Loader.Update.Packs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel for the 
/// </summary>
public class InstallModPackEntryPageViewModel : ObservableObject
{
    /// <summary>
    /// Viewmodel that owns this VM.
    /// </summary>
    public InstallModPackDialogViewModel Owner { get; set; }

    /// <summary>
    /// Contains the pack that we will be installing.
    /// </summary>
    public ReloadedPack Pack { get; set; }

    /// <summary/>
    public InstallModPackEntryPageViewModel(InstallModPackDialogViewModel owner)
    {
        Owner = owner;
        Pack = owner.Pack;
    }

    /// <summary>
    /// Sets the index of the page displayed.
    /// </summary>
    public void SetPage(int i) => Owner.PageIndex = i;
}