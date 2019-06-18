using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using Reloaded.Mod.Launcher.Commands;
using Reloaded.Mod.Launcher.Models.Model;
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
        private static ConfigReader<ApplicationConfig> _applicationConfigReader = new ConfigReader<ApplicationConfig>();

        /* Fired after the Applications collection changes. */
        public event NotifyCollectionChangedEventHandler ApplicationsChanged = (sender, args) => { };

        /* The page to display. */
        public BaseSubPage Page { get; set; } = BaseSubPage.Welcome;

        /* Set this to false to temporarily suspend the file system watcher monitoring new applications. */
        public bool MonitorNewApplications { get; set; } = true;
        public ImageApplicationPathTuple SelectedApplication { get; set; }

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
        private FileSystemWatcher _applicationWatcher; /* Monitors for additions/changes in available applications. */

        /* Get Applications Task */
        private SerialTaskCommand _getApplicationsTaskCommand = new SerialTaskCommand();

        public MainPageViewModel()
        {
            GetApplications();

            string appConfigDirectory = LoaderConfigReader.ReadConfiguration().ApplicationConfigDirectory;
            _applicationWatcher = FileSystemWatcherFactory.CreateGeneric(appConfigDirectory, StartGetApplicationsTask, FileSystemWatcherFactory.FileSystemWatcherEvents.Changed | FileSystemWatcherFactory.FileSystemWatcherEvents.Renamed | FileSystemWatcherFactory.FileSystemWatcherEvents.Deleted, true, "*.json");
        }

        /// <summary>
        /// Manually raises the property changed event for the <see cref="Page"/> property.
        /// Used for specific pages that are not singletons/have only one instance.
        /// </summary>
        public void RaisePagePropertyChanged()
        {
            RaisePropertyChangedEvent(nameof(Page));
        }
        
        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications(CancellationToken cancellationToken = default)
        {
            var applications = new ObservableCollection<ImageApplicationPathTuple>();
            var loaderConfig = LoaderConfigReader.ReadConfiguration();
            List<PathGenericTuple<ApplicationConfig>> applicationConfigs;
                
            // Check for cancellation request before config reading begins if necessary. */
            if (cancellationToken.IsCancellationRequested)
                return;

            // Try read all configs, this action may sometimes fail if some of the files are still being copied.
            // Worth noting is that the last fired event will never collide here and fail, thus this is a safe point to exit.
            try { applicationConfigs = _applicationConfigReader.ReadConfigurations(loaderConfig.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName); }
            catch (Exception) { return; }

            foreach (var config in applicationConfigs)
                applications.Add(new ImageApplicationPathTuple(GetImageForAppConfig(config), config.Object, config.Path));

            Applications = applications;
        }

        private void StartGetApplicationsTask()
        {
            if (MonitorNewApplications)
                _getApplicationsTaskCommand.Execute(new Action<CancellationToken>(GetApplications));
        }

        /// <summary>
        /// Obtains an image to represent a given application.
        /// The image is either a custom one or the icon of the application.
        /// </summary>
        private ImageSource GetImageForAppConfig(PathGenericTuple<ApplicationConfig> applicationConfig)
        {
            // Check if custom icon exists.
            if (!String.IsNullOrEmpty(applicationConfig.Object.AppIcon))
            {
                string logoDirectory = Path.GetDirectoryName(applicationConfig.Path);
                string logoFilePath = Path.Combine(logoDirectory, applicationConfig.Object.AppIcon);

                if (File.Exists(logoFilePath))
                    return Misc.Imaging.BitmapFromUri(new Uri(logoFilePath, UriKind.Absolute));
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
            else
            {
                var image = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
                image.Freeze();

                return image;
            }
        }
    }
}
