using System.Collections.Generic;
using Reloaded.Mod.Loader.Community.Config;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Represents the dialog used to show warnings for the recently added application.
/// </summary>
public class AddApplicationWarningDialogViewModel : ObservableObject
{
    /// <summary>
    /// List of warnings to display to the user.
    /// </summary>
    public List<WarningItem> Warnings { get; set; }

    /// <inheritdoc />
    public AddApplicationWarningDialogViewModel(List<WarningItem> warnings)
    {
        Warnings = warnings;
    }
}