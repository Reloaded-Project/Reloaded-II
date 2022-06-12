using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using static Reloaded.Mod.Loader.IO.Utility.FileSystemWatcherFactory;

namespace Reloaded.Mod.Loader.IO.Services;

public abstract class ConfigServiceBase<TConfigType> where TConfigType : IConfig<TConfigType>, new()
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
    private SynchronizationContext _context = SynchronizationContext.Current;
    private Func<List<PathTuple<TConfigType>>> _getAllConfigs;

    /// <summary>
    /// Creates the service instance given an instance of the configuration.
    /// </summary>
    /// <param name="configDirectory">The directory containing subdirectories storing JSON configurations.</param>
    /// <param name="itemFileName">File name of an individual item.</param>
    /// <param name="getAllConfigs">A function that retrieves all configurations.</param>
    /// <param name="context">Context to which background events should be synchronized.</param>
    public void Initialize(string configDirectory, string itemFileName, Func<List<PathTuple<TConfigType>>> getAllConfigs, SynchronizationContext context = null)
    {
        bool executeImmediately = context == null;
        _context = context ?? _context;

        ConfigDirectory = configDirectory;
        ItemFileName    = itemFileName;
        _getAllConfigs  = getAllConfigs;
        _renameWatcher          = Create(ConfigDirectory, null, OnRename, FileSystemWatcherEvents.Renamed, true, "*.json");
        _createFolderWatcher    = Create(ConfigDirectory, OnCreateFolder, null, FileSystemWatcherEvents.Created, false, "*.*");
        _createFileWatcher      = Create(ConfigDirectory, OnCreateFile, null, FileSystemWatcherEvents.Created, true, "*.json");
        _changedWatcher         = Create(ConfigDirectory, OnUpdateFile, null, FileSystemWatcherEvents.Changed, true, "*.json");
        _deleteFileWatcher      = Create(ConfigDirectory, OnDeleteFile, null, FileSystemWatcherEvents.Deleted);
        _deleteDirectoryWatcher = Create(ConfigDirectory, OnDeleteDirectory, null, FileSystemWatcherEvents.Deleted, false, "*.*");
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

    private void OnCreateFolder(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(e.FullPath))
            return;

        var configFullPath = Path.Combine(e.FullPath, ItemFileName);
        if (!File.Exists(configFullPath))
            return;

        var config = IConfig<TConfigType>.FromPath(configFullPath);
        _context.Post(() => AddItem(new PathTuple<TConfigType>(configFullPath, config)));
    }

    private void OnCreateFile(object sender, FileSystemEventArgs e) => CreateFileHandler(e.FullPath);

    private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
    {
        if (ItemsByFolder.TryGetValue(e.FullPath, out var deletedMod))
        {
            _context.Post(() =>
            {
                RemoveItem(deletedMod);
            });
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

    private void AddItem(PathTuple<TConfigType> itemTuple)
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
        var pathContainingFolder = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
        return pathContainingFolder.Equals(ConfigDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsFileConfigFile(string filePath)
    {
        return Path.GetFileName(filePath).Equals(ItemFileName, StringComparison.OrdinalIgnoreCase);
    }
}