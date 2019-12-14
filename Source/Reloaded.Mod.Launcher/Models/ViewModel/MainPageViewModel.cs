using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;
using Reloaded.WPF.Utilities;
using static Reloaded.Mod.Launcher.Utility.ActionWrappers;
using static Reloaded.Mod.Loader.IO.FileSystemWatcherFactory;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class MainPageViewModel : ObservableObject
    {
        /* Fired after the Applications collection changes. */
        public event NotifyCollectionChangedEventHandler ApplicationsChanged = (sender, args) => { };

        /* The page to display. */
        public BaseSubPage Page
        {
            get => _baseSubPage;
            set
            {
                if (_baseSubPage != value)
                {
                    _baseSubPage = value;
                    RaisePropertyChangedEvent(nameof(Page));
                }
            }
        }

        /* Set this to false to temporarily suspend the file system watcher monitoring new applications. */
        public ImageApplicationPathTuple SelectedApplication { get; private set; }

        /* List of programs on the sidebar. */
        public ObservableCollection<ImageApplicationPathTuple> Applications { get; set; }

        /* This one is to allow us to switch application without raising event twice. See: SwitchApplication */
        private BaseSubPage _baseSubPage = BaseSubPage.SettingsPage;

        /* Application Monitoring */
        private readonly FileSystemWatcher _createWatcher;
        private readonly FileSystemWatcher _deleteFileWatcher;
        private readonly FileSystemWatcher _deleteDirectoryWatcher;

        private AutoInjector _autoInjector;

        /* Get Applications Task */
        private CancellableExecuteActionTimer _getApplicationsActionTimer = new CancellableExecuteActionTimer(new XamlResource<int>("RefreshApplicationsEventTickTimer").Get());

        public MainPageViewModel()
        {
            Applications = new ObservableCollection<ImageApplicationPathTuple>();
            Applications.CollectionChanged += (sender, args) => { ApplicationsChanged(sender, args); };
            GetApplications();

            string appConfigDirectory = IoC.Get<LoaderConfig>().ApplicationConfigDirectory;
            _createWatcher = CreateGeneric(appConfigDirectory, StartGetApplicationsTask, FileSystemWatcherEvents.Changed | FileSystemWatcherEvents.Created);
            _deleteFileWatcher = CreateChangeCreateDelete(appConfigDirectory, OnDeleteFile, FileSystemWatcherEvents.Deleted);
            _deleteDirectoryWatcher = CreateChangeCreateDelete(appConfigDirectory, OnDeleteDirectory, FileSystemWatcherEvents.Deleted, false, "*.*");

            _autoInjector = new AutoInjector(this);
        }

        /// <summary>
        /// Changes the Application page to display and displays the application.
        /// </summary>
        public void SwitchToApplication(ImageApplicationPathTuple tuple)
        {
            SelectedApplication = tuple;
            _baseSubPage = BaseSubPage.Application;
            RaisePropertyChangedEvent(nameof(Page));
        }

        // == Events ==
        private void OnDeleteDirectory(object sender, FileSystemEventArgs e)
        {
            // Handles deleted directories containing mod configurations.
            // Remove any mod that may have been inside removed directory.
            ExecuteWithApplicationDispatcher(() =>
            {
                // Copy in case Mods collection changes.
                var allApps = Applications;
                var deletedApps = allApps.Where(x => Path.GetDirectoryName(x.ConfigPath).Equals(e.FullPath));
                ExecuteWithApplicationDispatcherAsync(() =>
                {
                    foreach (var deletedApp in deletedApps)
                        Applications.Remove(deletedApp);
                });
            });
        }

        private void OnDeleteFile(object sender, FileSystemEventArgs e)
        {
            // Handles deleted mod configuration files.
            // Remove any mod that matches the path of a deleted config file.
            ExecuteWithApplicationDispatcher(() =>
            {
                // Copy in case Mods collection changes.
                var allApps = Applications;
                var deletedApps = allApps.Where(x => x.ConfigPath.Equals(e.FullPath));
                ExecuteWithApplicationDispatcherAsync(() =>
                {
                    foreach (var deletedApp in deletedApps)
                        Applications.Remove(deletedApp);
                });
            });
        }

        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications(CancellationToken cancellationToken = default)
        {
            try
            {
                var appConfigs = ApplicationConfig.GetAllApplications(IoC.Get<LoaderConfig>().ApplicationConfigDirectory, cancellationToken);
                if (! cancellationToken.IsCancellationRequested)
                {
                    var apps = appConfigs.Select(x => new ImageApplicationPathTuple(GetImageForAppConfig(x), x.Object, x.Path));
                    ExecuteWithApplicationDispatcher(() => Collections.ModifyObservableCollection(Applications, apps));
                }
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        /// <summary>
        /// Obtains an image to represent a given application.
        /// The image is either a custom one or the icon of the application.
        /// </summary>
        private ImageSource GetImageForAppConfig(PathGenericTuple<ApplicationConfig> applicationConfig)
        {
            // Check if custom icon exists.
            if (!string.IsNullOrEmpty(applicationConfig.Object.AppIcon))
            {
                if (ApplicationConfig.TryGetApplicationIcon(applicationConfig.Path, applicationConfig.Object, out var applicationIcon)) 
                    return Misc.Imaging.BitmapFromUri(new Uri(applicationIcon, UriKind.Absolute));
            }
            
            // Otherwise extract new icon from executable.
            if (File.Exists(applicationConfig.Object.AppLocation))
            {
                // Else make new from icon.
                using (Icon ico = Icon.ExtractAssociatedIcon(applicationConfig.Object.AppLocation))
                {
                    // Extract to config set location.
                    BitmapSource bitmapImage = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }

            var image = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
            image.Freeze();

            return image;
        }

        private void StartGetApplicationsTask() => _getApplicationsActionTimer.SetActionAndReset(GetApplications);
    }
}
