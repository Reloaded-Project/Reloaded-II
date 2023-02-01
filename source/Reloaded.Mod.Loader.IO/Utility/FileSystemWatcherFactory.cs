namespace Reloaded.Mod.Loader.IO.Utility;

/// <summary>
/// Factory class which provides convenience methods to create instances of <see cref="FileSystemWatcher"/>.
/// </summary>
public static class FileSystemWatcherFactory
{
    /// <summary>
    /// A factory method that creates a <see cref="FileSystemWatcher"/> which calls a specified method
    /// <see cref="Action{T}"/> when files at a given path change.
    /// </summary>
    /// <param name="configDirectory">The path of the directory containing the configurations.</param>
    /// <param name="action">The function to run when a configuration is altered or changed.</param>
    /// <param name="renamedEventHandler">The function to run when an item is renamed.</param>
    /// <param name="events">The events which trigger the launching of given action.</param>
    /// <param name="enableSubdirectories">Decides whether subdirectories in a given path should be monitored.</param>
    /// <param name="filter">The filter used to determine which files are being watched for.</param>
    /// <param name="useBigBuffers">If true, uses a big internal buffer for receiving changes.</param>
    public static FileSystemWatcher Create(string configDirectory, FileSystemEventHandler action, RenamedEventHandler renamedEventHandler, FileSystemWatcherEvents events, bool enableSubdirectories = true, string filter = "*.json", bool useBigBuffers = true)
    {
        var watcher = new FileSystemWatcher(configDirectory);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = enableSubdirectories;
        watcher.Filter = filter;
        watcher.NotifyFilter = 0;

        if (useBigBuffers)
            watcher.InternalBufferSize = 65536;

        if (events.HasFlag(FileSystemWatcherEvents.Deleted))
        {
            watcher.Deleted += action;
            watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        }

        if (events.HasFlag(FileSystemWatcherEvents.Changed))
        {
            watcher.Changed += action;
            watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
        }

        if (events.HasFlag(FileSystemWatcherEvents.Created))
        {
            watcher.Created += action;
            watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        }

        if (events.HasFlag(FileSystemWatcherEvents.Renamed))
        {
            watcher.Renamed += renamedEventHandler;
            watcher.NotifyFilter |= NotifyFilters.DirectoryName | NotifyFilters.FileName;
        }

        return watcher;
    }

    [Flags]
    public enum FileSystemWatcherEvents
    {
        Deleted = 0b00000001,
        Changed = 0b00000010,
        Created = 0b00000100,
        Renamed = 0b00001000
    }
}