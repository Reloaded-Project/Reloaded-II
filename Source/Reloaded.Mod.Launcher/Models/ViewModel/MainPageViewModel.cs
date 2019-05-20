using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Pages;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class MainPageViewModel : ObservableObject
    {
        private static LoaderConfigReader _loaderConfigReader = new LoaderConfigReader();
        private static ConfigLoader<ApplicationConfig> _applicationConfigLoader = new ConfigLoader<ApplicationConfig>();

        /// <summary> A persistent cache of all the icons of executables. </summary>
        private static Dictionary<string, BitmapSource> _iconCache = new Dictionary<string, BitmapSource>();

        /* Fired after the Applications collection changes. */
        public event NotifyCollectionChangedEventHandler ApplicationsChanged = (sender, args) => { };

        /* The page to display. */
        public BaseSubPage Page { get; set; } = BaseSubPage.Welcome;

        /* Set this to false to temporarily suspend the file system watcher monitoring new applications. */
        public bool MonitorNewApplications { get; set; } = true;

        /* List of programs on the sidebar. */
        [DoNotNotify]
        public ObservableCollection<ImageApplicationPathTuple> Applications
        {
            get => _applications;
            set
            {
                value.CollectionChanged += ApplicationsChanged;
                _applications = value;

                RaisePropertyChangedEvent(nameof(Applications));
                ApplicationsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        } 

        /* Application Monitoring */
        private ObservableCollection<ImageApplicationPathTuple> _applications;
        private FileSystemWatcher _fileSystemWatcher; /* Monitors for additions/changes in available applications. */

        /* Get Applications Task */
        private Task _getApplicationsTask;
        private CancellationToken _getApplicationsCancellationToken;
        private CancellationTokenSource _getApplicationsCancellationTokenSource;

        public MainPageViewModel()
        {
            GetApplications();

            // Observe changes to filesystem to automatically recognize new entries.
            SetupApplicationWatch();
        }

        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications()
        {
            var applications = new ObservableCollection<ImageApplicationPathTuple>();
            var loaderConfig = _loaderConfigReader.ReadConfiguration();
            List<PathGenericTuple<ApplicationConfig>> applicationConfigs;
                
            /* Check for cancellation request before config reading begins if necessary. */
            if (_getApplicationsCancellationToken.IsCancellationRequested)
                return;

            /* Try read all configs, this action may sometimes fail if some of the files are still being copied.
               Worth noting is that the last fired event will never collide here and fail, thus this is a safe point to exit.
            */
            try { applicationConfigs = _applicationConfigLoader.ReadConfigurations(loaderConfig.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName); }
            catch (Exception) { return; }

            foreach (var config in applicationConfigs)
                applications.Add(new ImageApplicationPathTuple(GetImageForModConfig(config), config.Object, config.Path));

            Applications = applications;
        }

        /// <summary>
        /// Sets up an observer that automatically updates applications as they are created/deleted/renamed.
        /// </summary>
        private void SetupApplicationWatch()
        {
            _fileSystemWatcher = new FileSystemWatcher(_loaderConfigReader.ReadConfiguration().ApplicationConfigDirectory);
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.Deleted += (a, b) => { StartGetApplicationsTask(); };
            _fileSystemWatcher.Changed += (a, b) => { StartGetApplicationsTask(); };
            _fileSystemWatcher.Renamed += (a, b) => { StartGetApplicationsTask(); };
        }

        private void StartGetApplicationsTask()
        {
            if (MonitorNewApplications)
            {
                // Cancel current task 
                if (_getApplicationsTask != null)
                {
                    _getApplicationsCancellationTokenSource.Cancel();
                    try { _getApplicationsTask.Wait(_getApplicationsCancellationToken); }
                    catch (Exception) { /* ignored */ }
                }

                _getApplicationsCancellationTokenSource = new CancellationTokenSource();
                _getApplicationsCancellationToken = _getApplicationsCancellationTokenSource.Token;
                _getApplicationsTask = Task.Run(GetApplications, _getApplicationsCancellationToken);
            }
        }

        /// <summary>
        /// Obtains an image to represent a given application.
        /// The image is either a custom one or the icon of the application.
        /// </summary>
        /// <param name="applicationConfig">Individual configuration of the application.</param>
        private ImageSource GetImageForModConfig(PathGenericTuple<ApplicationConfig> applicationConfig)
        {
            // Check if custom icon exists.
            if (!String.IsNullOrEmpty(applicationConfig.Object.AppIcon))
            {
                string logoDirectory = Path.GetDirectoryName(applicationConfig.Path);
                string logoFilePath = Path.Combine(logoDirectory, applicationConfig.Object.AppIcon);

                if (File.Exists(logoFilePath))
                {
                    return new BitmapImage(new Uri(logoFilePath, UriKind.Absolute));
                }
            }
            
            // Otherwise extract new icon from executable.
            if (File.Exists(applicationConfig.Object.AppLocation))
            {
                // Try to retrieve from cache.
                if (_iconCache.TryGetValue(applicationConfig.Object.AppLocation, out BitmapSource icon))
                    return icon;

                // Else make new from icon.
                using (Icon ico = Icon.ExtractAssociatedIcon(applicationConfig.Object.AppLocation))
                {
                    // Extract to config set location.
                    BitmapSource bitmapImage = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    CachedBitmap cachedBitmap = new CachedBitmap(bitmapImage, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    cachedBitmap.Freeze();
                    bitmapImage.Freeze();

                    _iconCache[applicationConfig.Object.AppLocation] = cachedBitmap;
                    return bitmapImage;
                }
            }
            else
            {
                // As last resort, use Reloaded Icon. (Cached)
                if (_iconCache.TryGetValue(Paths.PLACEHOLDER_IMAGE, out BitmapSource icon))
                    return icon;

                var image = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
                var cachedBitmap = new CachedBitmap(image, BitmapCreateOptions.None, BitmapCacheOption.Default);
                cachedBitmap.Freeze();
                image.Freeze();

                _iconCache[Paths.PLACEHOLDER_IMAGE] = cachedBitmap;
                return image;
            }
        }
    }
}
