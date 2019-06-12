using System;
using System.IO;

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// Factory class which provides convenience methods to create instances of <see cref="FileSystemWatcher"/>.
    /// </summary>
    public static class FileSystemWatcherFactory
    {
        /// <summary>
        /// A general "one size fits all" factory method that creates a <see cref="FileSystemWatcher"/> which calls a specified method
        /// <see cref="action"/> when files at a given path change.
        /// </summary>
        /// <param name="configDirectory">The path to monitor.</param>
        /// <param name="action">The function to run.</param>
        /// <param name="events">The events which trigger the action.</param>
        /// <param name="enableSubdirectories">Decides whether subdirectories in a given path should be monitored.</param>
        /// <param name="filter">The filter used to determine which files are being watched for.</param>
        public static FileSystemWatcher CreateGeneric(string configDirectory, Action action, FileSystemWatcherEvents events, bool enableSubdirectories = true, string filter = "*.json")
        {
            var watcher = new FileSystemWatcher(configDirectory);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = enableSubdirectories;
            watcher.Filter = filter;

            if (events.HasFlag(FileSystemWatcherEvents.Deleted))
                watcher.Deleted += (a, b) => { action(); };

            if (events.HasFlag(FileSystemWatcherEvents.Changed))
                watcher.Changed += (a, b) => { action(); };

            if (events.HasFlag(FileSystemWatcherEvents.Renamed))
                watcher.Renamed += (a, b) => { action(); };

            if (events.HasFlag(FileSystemWatcherEvents.Created))
                watcher.Created += (a, b) => { action(); };

            return watcher;
        }

        /// <summary>
        /// A factory method that creates a <see cref="FileSystemWatcher"/> which calls a specified method
        /// <see cref="action"/> when files at a given path change.
        /// </summary>
        /// <param name="configDirectory">The path of the directory containing the configurations.</param>
        /// <param name="action">The function to run when a configuration is altered or changed.</param>
        /// <param name="events">The events which trigger the launching of given action.</param>
        /// <param name="enableSubdirectories">Decides whether subdirectories in a given path should be monitored.</param>
        /// <param name="filter">The filter used to determine which files are being watched for.</param>
        public static FileSystemWatcher CreateChangeCreateDelete(string configDirectory, FileSystemEventHandler action, FileSystemWatcherEvents events, bool enableSubdirectories = true, string filter = "*.json")
        {
            var watcher = new FileSystemWatcher(configDirectory);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = enableSubdirectories;
            watcher.Filter = filter;

            if (events.HasFlag(FileSystemWatcherEvents.Deleted))
                watcher.Deleted += action;

            if (events.HasFlag(FileSystemWatcherEvents.Changed))
                watcher.Changed += action;

            if (events.HasFlag(FileSystemWatcherEvents.Created))
                watcher.Created += action;

            if (events.HasFlag(FileSystemWatcherEvents.Renamed))
                throw new ArgumentException($"{FileSystemWatcherEvents.Renamed} is not supported.");

            return watcher;
        }

        /// <summary>
        /// A factory method that creates a <see cref="FileSystemWatcher"/> which calls a specified method
        /// <see cref="action"/> when configurations within a given directory change.
        /// </summary>
        /// <param name="configDirectory">The path of the directory containing the configurations.</param>
        /// <param name="action">The function to run when a configuration is altered or changed.</param>
        /// <param name="enableSubdirectories">Decides whether subdirectories in a given path should be monitored.</param>
        /// <param name="filter">The filter used to determine which files are being watched for.</param>
        public static FileSystemWatcher CreateRename(string configDirectory, RenamedEventHandler action, bool enableSubdirectories = true, string filter = "*.json")
        {
            var watcher = new FileSystemWatcher(configDirectory);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = enableSubdirectories;
            watcher.Filter = filter;
            watcher.Renamed += action;

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
}
