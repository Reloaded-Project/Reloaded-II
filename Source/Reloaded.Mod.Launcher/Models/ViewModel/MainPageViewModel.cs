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
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;
using Reloaded.WPF.Utilities;

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
        public bool MonitorNewApplications { get; set; } = true;
        public ImageApplicationPathTuple SelectedApplication { get; set; }

        /* List of programs on the sidebar. */
        #pragma warning disable CS0436 // Type conflicts with imported type
        [DoNotNotify] // Conflict is intended, so we don't have reference to PropertyChanged after building. Fody will fix conflict at compile time.
        #pragma warning restore CS0436 // Type conflicts with imported type
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

        /* This one is to allow us to switch application without raising event twice. See: SwitchApplication */
        private BaseSubPage _baseSubPage = BaseSubPage.SettingsPage;

        /* Application Monitoring */
        private ObservableCollection<ImageApplicationPathTuple> _applications;
        private FileSystemWatcher _applicationWatcher; /* Monitors for additions/changes in available applications. */
        private AutoInjector _autoInjector;

        /* Get Applications Task */
        private CancellableExecuteActionTimer _getApplicationsActionTimer = new CancellableExecuteActionTimer(new XamlResource<int>("RefreshApplicationsEventTickTimer").Get());

        public MainPageViewModel()
        {
            GetApplications();

            string appConfigDirectory = IoC.Get<LoaderConfig>().ApplicationConfigDirectory;
            _applicationWatcher = FileSystemWatcherFactory.CreateGeneric(appConfigDirectory, StartGetApplicationsTask, FileSystemWatcherFactory.FileSystemWatcherEvents.Changed | FileSystemWatcherFactory.FileSystemWatcherEvents.Renamed | FileSystemWatcherFactory.FileSystemWatcherEvents.Deleted);
            _autoInjector = new AutoInjector(this);
        }

        /// <summary>
        /// Changes the <see cref="Page"/> property to <see cref="BaseSubPage.Application"/> and
        /// raises the property changed event.
        /// </summary>
        public void SwitchToApplication()
        {
            _baseSubPage = BaseSubPage.Application;
            RaisePropertyChangedEvent(nameof(Page));
        }
        
        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications(CancellationToken cancellationToken = default)
        {
            var applications = new ObservableCollection<ImageApplicationPathTuple>();
            List<PathGenericTuple<ApplicationConfig>> applicationConfigs;
                
            // Check for cancellation request before config reading begins if necessary. */
            if (cancellationToken.IsCancellationRequested)
                return;

            // Try read all configs, this action may sometimes fail if some of the files are still being copied.
            // Worth noting is that the last fired event will never collide here and fail, thus this is a safe point to exit.
            try   { applicationConfigs = ApplicationConfig.GetAllApplications(IoC.Get<LoaderConfig>().ApplicationConfigDirectory, cancellationToken); }
            catch (Exception) { return; }

            foreach (var config in applicationConfigs)
                applications.Add(new ImageApplicationPathTuple(GetImageForAppConfig(config), config.Object, config.Path));

            ActionWrappers.ExecuteWithApplicationDispatcher(() => Collections.UpdateObservableCollection(ref _applications, applications));
        }

        private void StartGetApplicationsTask()
        {
            if (MonitorNewApplications)
                _getApplicationsActionTimer.SetActionAndReset(GetApplications);
        }

        public void InvokeWithoutMonitoringApplications(Action action)
        {
            MonitorNewApplications = false;
            action();
            MonitorNewApplications = true;
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
                var hasIcon = ApplicationConfig.TryGetApplicationIcon(applicationConfig.Path, applicationConfig.Object, out var applicationIcon);

                if (hasIcon)
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
            else
            {
                var image = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.RelativeOrAbsolute));
                image.Freeze();

                return image;
            }
        }
    }
}
