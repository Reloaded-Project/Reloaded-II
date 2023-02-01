using Reloaded.Mod.Loader.Update.Packs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel for the page that allows user to select if they wish to install a mod or not.
/// </summary>
public class InstallModPackModPageViewModel : ObservableObject
{
    /// <summary>
    /// Viewmodel that owns this VM.
    /// </summary>
    public InstallModPackDialogViewModel Owner { get; set; }

    /// <summary>
    /// The item displayed in this page.
    /// </summary>
    public BooleanGenericTuple<ReloadedPackItem> Item { get; set; }

    /// <summary>
    /// The viewmodel for installing a mod in a given mod pack.
    /// </summary>
    /// <param name="owner">The parent viewmodel.</param>
    /// <param name="item">The item to be displayed in the UI.</param>
    public InstallModPackModPageViewModel(InstallModPackDialogViewModel owner, BooleanGenericTuple<ReloadedPackItem> item)
    {
        Owner = owner;
        Item = item;
    }

    /// <summary>
    /// Sets the state of the item to enabled.
    /// </summary>
    public void SetEnabled() => Item.Enabled = true;

    /// <summary>
    /// Sets the state of the item to disabled.
    /// </summary>
    public void SetDisabled() => Item.Enabled = false;

    /// <summary>
    /// Advances the menu to the next page.
    /// </summary>
    public void NextPage() => Owner.PageIndex += 1;

    /// <summary>
    /// Advances the menu to the previous page.
    /// </summary>
    public void LastPage() => Owner.PageIndex -= 1;
}