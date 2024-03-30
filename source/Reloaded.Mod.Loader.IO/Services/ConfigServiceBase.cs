using static Reloaded.Mod.Loader.IO.Utility.FileSystemWatcherFactory;

namespace Reloaded.Mod.Loader.IO.Services;

public abstract class ConfigServiceBase<TConfigType> : ObservableObject where TConfigType : IConfig<TConfigType>, new()
{
    /// <summary>
    /// Current up to date list of all items.
    /// </summary>
    public ObservableCollection<PathTuple<TConfigType>> Items { get; set; } = new ObservableCollection<PathTuple<TConfigType>>();

    /// <summary>
    /// Current mapping of all items by their physical on disk location.
    /// </summary>
    public Dictionary<string, PathTuple<TConfigType>> ItemsByPath { get; set; } = new Dictionary<string, PathTuple<TConfigType>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Current mapping of all items by their physical on disk folder.
    /// </summary>
    public Dictionary<string, PathTuple<TConfigType>> ItemsByFolder { get; set; } = new Dictionary<string, PathTuple<TConfigType>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The folder containing all of the folders of mod profiles.
    /// </summary>
    public string ConfigDirectory { get; private set; }

    /// <summary>
    /// File name of an individual config item.
    /// </summary>
    public string ItemFileName { get; private set; }

    public event Action<PathTuple<TConfigType>> OnAddItem;
    public event Action<PathTuple<TConfigType>> OnRemoveItem;

    /* Mod Monitoring */
    private FileSystemWatcher _renameWatcher;
    private FileSystemWatcher _changedWatcher;
    private FileSystemWatcher _createFolderWatcher;
    private FileSystemWatcher _createFileWatcher;
    private FileSystemWatcher _deleteFileWatcher;
    private FileSystemWatcher _deleteDirectoryWatcher;
    private FileSystemWatcher _renameDirectoryWatcher;
    private SynchronizationContext _context = SynchronizationContext.Current;
    private Func<List<PathTuple<TConfigType>>> _getAllConfigs;

    /// <summary>
    /// Creates the service instance given an instance of the configuration.
    /// </summary>
    /// <param name="configDirectory">The directory containing subdirectories storing JSON configurations.</param>
    /// <param name="itemFileName">File name of an individual item.</param>
    /// <param name="getAllConfigs">A function that retrieves all configurations.</param>
    /// <param name="context">Context to which background events should be synchronized.</param>
    /// <param name="useBigBuffers">True to use large <see cref="FileSystemWatcher"/> buffers.</param>
    public void Initialize(string configDirectory, string itemFileName, Func<List<PathTuple<TConfigType>>> getAllConfigs, SynchronizationContext context = null, bool useBigBuffers = false)
    {
        bool executeImmediately = context == null;
        _context = context ?? _context;

        ConfigDirectory = configDirectory;
        ItemFileName    = itemFileName;
        _getAllConfigs  = getAllConfigs;
        _renameWatcher          = Create(ConfigDirectory, null, OnRename, FileSystemWatcherEvents.Renamed, true, "*.json", useBigBuffers);
        _createFolderWatcher    = Create(ConfigDirectory, OnCreateFolder, null, FileSystemWatcherEvents.Created, true, "*.*", useBigBuffers);
        _createFileWatcher      = Create(ConfigDirectory, OnCreateFile, null, FileSystemWatcherEvents.Created, true, "*.json", useBigBuffers);
        _changedWatcher         = Create(ConfigDirectory, OnUpdateFile, null, FileSystemWatcherEvents.Changed, true, "*.json", useBigBuffers);
        _deleteFileWatcher      = Create(ConfigDirectory, OnDeleteFile, null, FileSystemWatcherEvents.Deleted, true, useBigBuffers: useBigBuffers);
        _deleteDirectoryWatcher = Create(ConfigDirectory, OnDeleteDirectory, null, FileSystemWatcherEvents.Deleted, true, "*.*", useBigBuffers);
        _renameDirectoryWatcher = Create(ConfigDirectory, null, OnRenameDirectory, FileSystemWatcherEvents.Renamed, true, "*.*", useBigBuffers);
        GetItems(executeImmediately);
    }

    /// <summary>
    /// Attempts to move the folder used for storing mod profiles to a new directory.
    /// </summary>
    /// <param name="newDirectory">Path to the new directory.</param>
    public void MoveFolder(string newDirectory)
    {
        throw new NotImplementedException("This feature will be implemented in the future.");
    }
    
    /// <summary>
    /// Populates the mod list governed by <see cref="Items"/>.
    /// </summary>
    public virtual void ForceRefresh()
    {
        bool executeImmediately = _context == null;
        GetItems(executeImmediately);
    }

    /// <summary>
    /// Populates the mod list governed by <see cref="Items"/>.
    /// </summary>
    private void GetItems(bool executeImmediately, CancellationToken cancellationToken = default)
    {
        try
        {
            var itemTuples = _getAllConfigs();

            // Set new collection of mods by path.
            ItemsByPath.Clear();
            ItemsByFolder.Clear();
            foreach (var item in itemTuples)
            {
                ItemsByPath[item.Path] = item;
                ItemsByFolder[Path.GetDirectoryName(item.Path)] = item;
            }

            if (executeImmediately)
            {
                Collections.ModifyObservableCollection(Items, itemTuples);
            }
            else
            {
                _context.Send((x) =>
                {
                    Collections.ModifyObservableCollection(Items, itemTuples);
                }, null);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void OnRename(object sender, RenamedEventArgs e)
    {
        // Add item if this is a new config file.
        CreateFileHandler(e.FullPath);

        // Remove item if renamed from valid config file.
        if (ItemsByPath.TryGetValue(e.OldFullPath, out var mod))
            _context.Post(() => RemoveItem(mod));
    }

    private void OnRenameDirectory(object sender, RenamedEventArgs e)
    {
        if (!Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath))
            return;

        // Trigger the deletion of the previous folder.
        OnDeleteDirectory(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, e.OldFullPath, null));

        // Trigger the creation of the new folder.
        OnCreateFolder(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, e.FullPath, null));
        
        // Handling this in this way is needed as mods can be nested.
    }

    private void OnCreateFolder(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(e.FullPath))
            return;

        // Read configurations from subdirectories within the current path.
        var configs = ConfigReader<TConfigType>.ReadConfigurations(Path.TrimEndingDirectorySeparator(e.FullPath), ItemFileName, maxDepth: int.MaxValue, recurseOnFound: false);
        foreach (var config in configs)
        {
            _context.Post(() => AddItem(config));
        }
    }

    private void OnCreateFile(object sender, FileSystemEventArgs e) => CreateFileHandler(e.FullPath);

    private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
    {
        var deletedPath = e.FullPath;

        // Fast path: If we're directly deleting a known folder, then there can not be any subfolders,
        // as we don't allow mods inside mods.
        var isDirectFolder = ItemsByFolder.TryGetValue(deletedPath, out var directMod);
        if (isDirectFolder)
        {
            _context.Post(() =>
            {
                RemoveItem(directMod);
            });
            return;
        }
        
        // We need paths with trailing slashes in the case of:
        // - Mod A/ModZ
        // - Mod A Extra/ModZ
        // We don't want to match 'Mod A Extra' when we're looking for 'Mod A'.
        if (!Path.EndsInDirectorySeparator(deletedPath))
            deletedPath += Path.DirectorySeparatorChar;
        
        // Otherwise iterate over all possible subfolders.
        // This is a bit inefficient, but with nested mods, it's the only way (without creating a whole tree of nodes).
        // Can rack up upwards of 20ms in huge mod directories.
        foreach (var item in ItemsByFolder)
        {
            var modFolder = item.Key;
            if (!Path.EndsInDirectorySeparator(modFolder))
                modFolder += Path.DirectorySeparatorChar;
            
            var shouldRemove = modFolder.StartsWith(deletedPath, StringComparison.OrdinalIgnoreCase);
            if (shouldRemove)
            { 
                _context.Post(() =>
                {
                    RemoveItem(item.Value);
                });
            }
        }
    }

    private void OnDeleteFile(object sender, FileSystemEventArgs e)
    {
        if (ItemsByPath.TryGetValue(e.FullPath, out var mod))
        {
            _context.Post(() =>
            {
                RemoveItem(mod);
            });
        }
    }

    private void CreateFileHandler(string fullPath)
    {
        if (IsFileInItemFolder(fullPath) && IsFileConfigFile(fullPath))
        {
            var config = IConfig<TConfigType>.FromPath(fullPath);
            _context.Post(() => AddItem(new PathTuple<TConfigType>(fullPath, config)));
        }
    }

    private void OnUpdateFile(object sender, FileSystemEventArgs e)
    {
        var fullPath = e.FullPath;
        if (IsFileInItemFolder(fullPath) && IsFileConfigFile(fullPath))
        {
            var config = IConfig<TConfigType>.FromPath(fullPath);
            _context.Post(() => AddItem(new PathTuple<TConfigType>(fullPath, config)));
        }
    }

    protected virtual void AddItem(PathTuple<TConfigType> itemTuple)
    {
        // Check for existing item.
        bool alreadyHasItem = ItemsByPath.TryGetValue(itemTuple.Path, out var existing);
        
        ItemsByPath[itemTuple.Path] = itemTuple;
        ItemsByFolder[Path.GetDirectoryName(itemTuple.Path)] = itemTuple;
        if (alreadyHasItem)
        {
            // Sometimes you might get directory, then file notification, so we might get a duplicate.
            // We hackily filter out this duplicate here.
            var index = Items.IndexOf(existing);
            Items[index] = itemTuple;
        }
        else
        {
            Items.Add(itemTuple);
            OnAddItem?.Invoke(itemTuple);
        }
    }

    private void RemoveItem(PathTuple<TConfigType> itemTuple)
    {
        ItemsByPath.Remove(itemTuple.Path);
        ItemsByFolder.Remove(Path.GetDirectoryName(itemTuple.Path));
        Items.Remove(itemTuple);
        OnRemoveItem?.Invoke(itemTuple);
    }

    private bool IsFileInItemFolder(string filePath)
    {
        var cfgPath = ConfigDirectory;
        var pathContainingFolder = Path.GetDirectoryName(Path.GetDirectoryName(filePath));

        // We need paths with trailing slashes in the case of:
        // - Mod A/ModZ
        // - Mod A Extra/ModZ
        // We don't want to match 'Mod A Extra' when we're looking for 'Mod A'.
        if (!Path.EndsInDirectorySeparator(cfgPath))
            cfgPath += Path.DirectorySeparatorChar;

        if (!Path.EndsInDirectorySeparator(pathContainingFolder))
            pathContainingFolder += Path.DirectorySeparatorChar;

        return pathContainingFolder.StartsWith(cfgPath, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsFileConfigFile(string filePath)
    {
        return Path.GetFileName(filePath).Equals(ItemFileName, StringComparison.OrdinalIgnoreCase);
    }
}