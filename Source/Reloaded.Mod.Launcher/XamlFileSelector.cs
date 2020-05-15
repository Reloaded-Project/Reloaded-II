using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Reloaded.Mod.Launcher.Annotations;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using static Reloaded.Mod.Loader.IO.FileSystemWatcherFactory.FileSystemWatcherEvents;

namespace Reloaded.Mod.Launcher
{
    public class XamlFileSelector : ResourceDictionary, INotifyPropertyChanged
    {
        private const string XamlFilter = "*.xaml";

        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();
        public string File { get; set; }

        private string Directory { get; set; }
        private FileSystemWatcher _directoryWatcher;

        public XamlFileSelector(string directoryPath)
        {
            Directory = directoryPath;
            _directoryWatcher = FileSystemWatcherFactory.CreateChangeCreateDelete(Directory, OnAvailableXamlFilesUpdated, Changed | Created | Deleted, false, XamlFilter);
            this.PropertyChanged += OnPropertyChanged;
            PopulateXamlFiles();
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
            if (File != null)
                base.Source = new Uri(File, UriKind.RelativeOrAbsolute);
        }

        /* Events */

        /// <summary>
        /// Auto update dictionary on chosen XAML file.
        /// </summary>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(File))
                UpdateSource();
        }

        private void OnAvailableXamlFilesUpdated(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(PopulateXamlFiles);
        }


        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
