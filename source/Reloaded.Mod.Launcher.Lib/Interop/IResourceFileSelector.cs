namespace Reloaded.Mod.Launcher.Lib.Interop;

/// <summary>
/// Interface used to select an individual resource file to pull data from.
/// </summary>
public interface IResourceFileSelector : INotifyPropertyChanged
{
    /// <summary>
    /// List of all files that can be accessed.
    /// </summary>
    ObservableCollection<string> Files { get; set; }

    /// <summary>
    /// The currently selected XAML file to store.
    /// </summary>
    string? File { get; set; }

    /// <summary>
    /// Executed after a new XAML file has been set as the source.
    /// </summary>
    event Action NewFileSet;

    /// <summary>
    ///     Selects an existing available XAML file with a matching file name.
    /// </summary>
    void SelectXamlFileByName(string fileName);
}