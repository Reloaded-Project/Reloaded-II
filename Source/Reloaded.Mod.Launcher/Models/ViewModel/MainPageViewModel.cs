using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Pages;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class MainPageViewModel : ObservableObject
    {
        private static LoaderConfigReader _loaderConfigReader = new LoaderConfigReader();
        private static ConfigLoader<ApplicationConfig> _applicationConfigLoader = new ConfigLoader<ApplicationConfig>();

        /* The page to display alongside the sidebar. */
        public Page Page { get; set; } = Page.None;

        /* List of programs on the sidebar. */
        public ObservableCollection<Tuple<ImageSource, IApplicationConfig>> Applications { get; set; } = new ObservableCollection<Tuple<ImageSource, IApplicationConfig>>();

        public MainPageViewModel()
        {
            GetApplications();
        }

        /// <summary>
        /// Populates the application list governed by <see cref="Applications"/>.
        /// </summary>
        private void GetApplications()
        {
            var applications = new ObservableCollection<Tuple<ImageSource, IApplicationConfig>>();
            var loaderConfig = _loaderConfigReader.ReadConfiguration();
            var applicationConfigs = _applicationConfigLoader.ReadConfigurations(loaderConfig.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName);

            foreach (var config in applicationConfigs)
                applications.Add(new Tuple<ImageSource, IApplicationConfig>(GetImageForModConfig(config), config.Object));

            Applications = applications;
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
            
            // Otherwise make new icon from executable.
            using (Icon ico = Icon.ExtractAssociatedIcon(applicationConfig.Object.AppIcon))
            {
                BitmapSource bitmapImage  = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                CachedBitmap cachedBitmap = new CachedBitmap(bitmapImage, BitmapCreateOptions.None, BitmapCacheOption.Default);
                cachedBitmap.Freeze();

                return bitmapImage;
            }
        }


    }
}
