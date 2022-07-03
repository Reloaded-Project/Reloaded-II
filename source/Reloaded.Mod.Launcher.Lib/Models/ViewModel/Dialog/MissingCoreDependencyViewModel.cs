namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for displaying a list of missing .NET Core Dependencies.
/// </summary>
public class MissingCoreDependencyDialogViewModel : ObservableObject
{
    /// <summary>
    /// List of missing dependencies to run Reloaded.
    /// </summary>
    public ObservableCollection<MissingDependency> Dependencies { get; set; }

    /// <inheritdoc />
    public MissingCoreDependencyDialogViewModel(DependencyChecker deps)
    {
        var models = deps.Dependencies.Where(x => !x.Available).Select(x => new MissingDependency(x));
        Dependencies = new ObservableCollection<MissingDependency>(models);
    }
}