using static Reloaded.Mod.Loader.IO.Utility.FileSystemWatcherFactory.FileSystemWatcherEvents;

namespace Reloaded.Mod.Launcher.Utility;

/// <summary>
/// Class that encapsulates a folder to be watched for all available xaml files.
/// Where <see cref="File"/> is the currently selected file and <see cref="Files"/> is a list of all files.
/// </summary>
public class XamlFileSelector : ResourceDictionary, IResourceFileSelector
{
    private const string XamlFilter = "*.xaml";

    /// <summary>
    /// List of all files that can be accessed.
    /// </summary>
    public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();

    /// <summary>
    /// The currently selected XAML file to store.
    /// </summary>
    public string? File { get; set; }

    /// <summary>
    /// Executed after a new XAML file has been set as the source.
    /// </summary>
    public event Action? NewFileSet;

    private string Directory { get; set; }
    private FileSystemWatcher _directoryWatcher;

    /// <inheritdoc />
    public XamlFileSelector(string directoryPath)
    {
        Directory = directoryPath;
        _directoryWatcher = FileSystemWatcherFactory.Create(Directory, OnAvailableXamlFilesUpdated, null, Changed | Created | Deleted, false, XamlFilter);
        this.PropertyChanged += OnPropertyChanged;
        PopulateXamlFiles();
    }
    
    /// <summary>
    ///     Selects an existing available XAML file with a matching file name.
    /// </summary>
    public void SelectXamlFileByName(string fileName)
    {
        foreach (var file in Files)
        {
            if (Path.GetFileName(file) != fileName) 
                continue;
                
            File = file;
            break;
        }
    }

    private void PopulateXamlFiles()
    {
        var files = System.IO.Directory.GetFiles(Directory, XamlFilter, SearchOption.TopDirectoryOnly);
        Collections.ModifyObservableCollection(Files, files);
        if (!files.Contains(File))
            File = files.FirstOrDefault();
    }

    private void UpdateSource()
    {
        if (File == null) 
            return;

        Source = new Uri(File, UriKind.RelativeOrAbsolute);

        /*
            Cleanup old Dictionaries:

            Normally I wouldn't personally suggest running GC.Collect in user code however there's 
            potentially a lot of resources to clean up  in terms of memory space. Especially if e.g. 
            user loaded in complex images.

            As this in practice occurs over a theme or language switch, it should be largely unnoticeable to the end user.
        */
        NewFileSet?.Invoke();
    }

    /* Events */

    /// <summary>
    /// Auto update dictionary on chosen XAML file.
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(File))
            UpdateSource();
    }

    private void OnAvailableXamlFilesUpdated(object sender, FileSystemEventArgs e)
    {
        ActionWrappers.ExecuteWithApplicationDispatcher(PopulateXamlFiles);
    }


    #region PropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}