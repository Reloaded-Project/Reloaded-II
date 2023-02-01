namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for selecting an individual game from a selection of possible games.
/// </summary>
public class SelectAddedGameDialogViewModel : ObservableObject
{
    /// <summary>
    /// List of selectable applications.
    /// </summary>
    public List<IndexAppEntry> Entries { get; set; }

    /// <summary>
    /// Returns the selected entry.
    /// </summary>
    public IndexAppEntry SelectedEntry { get; set; }

    /// <summary/>
    public SelectAddedGameDialogViewModel(List<IndexAppEntry> applications)
    {
        Entries = applications;
        SelectedEntry = Entries[0];
    }
}