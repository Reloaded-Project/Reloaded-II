namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Add app hash mismatch.
/// </summary>
public class AddAppHashMismatchDialogViewModel : ObservableObject
{
    /// <summary/>
    public string BadHashDescription { get; set; }

    /// <inheritdoc />
    public AddAppHashMismatchDialogViewModel(string hashMismatchDescription)
    {
        BadHashDescription = hashMismatchDescription;
    }
}