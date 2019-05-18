using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using WindowViewModel = Reloaded.Mod.Launcher.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class SplashPage : ReloadedIIPage
    {
        #region XAML String Resource Constants
        private const string XAML_SplashCreatingDefaultConfig = "SplashCreatingDefaultConfig";
        private const string XAML_SplashCleaningConfigurations = "SplashCleaningConfigurations";
        private const string XAML_SplashLoadCompleteIn = "SplashLoadCompleteIn";
        #endregion XAML String Resource Constants

        private const int MinimumSplashDelay = 2000;
        private SplashViewModel _splashViewModel;
        private ResourceManipulator _manipulator;

        public SplashPage() : base()
        {  
            InitializeComponent();
            this.Loaded += AfterLoading;

            // Setup ViewModel
            _splashViewModel = new SplashViewModel();
            this.DataContext = _splashViewModel;
            _manipulator = new ResourceManipulator(this);
        }

        private void AfterLoading(object sender, RoutedEventArgs e)
        {
            // Start preparing everything on Splash Screen!
            Task.Run(SetupApplication);
        }

        private async void SetupApplication()
        {
            // Counter for max load time.
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Cleanup Configurations
            UpdateText(_manipulator.Get<string>(XAML_SplashCreatingDefaultConfig));
            var _loaderConfig = CreateNewConfigIfNotExist();

            // Cleaning up App/Loader/Mod Configurations
            UpdateText(_manipulator.Get<string>(XAML_SplashCleaningConfigurations));

            var modConfigLoader = new ConfigLoader<ModConfig>();
            var appConfigLoader = new ConfigLoader<ApplicationConfig>();
            var loaderConfigReader = new LoaderConfigReader();

            foreach (var modConfiguration in modConfigLoader.ReadConfigurations(_loaderConfig.ModConfigDirectory, ModConfig.ConfigFileName))
            {
                modConfiguration.Object.CleanupConfig();
                modConfigLoader.WriteConfiguration(modConfiguration.Path, modConfiguration.Object);
            }

            foreach (var appConfiguration in appConfigLoader.ReadConfigurations(_loaderConfig.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName))
            {
                appConfiguration.Object.CleanupConfig();
                appConfigLoader.WriteConfiguration(appConfiguration.Path, appConfiguration.Object);
            }

            var loaderConfiguration = loaderConfigReader.ReadConfiguration();
            loaderConfiguration.CleanupConfig();
            loaderConfigReader.WriteConfiguration(loaderConfiguration);

            // Wait until splash screen time.
            UpdateText($"{_manipulator.Get<string>(XAML_SplashLoadCompleteIn)} {watch.ElapsedMilliseconds}ms");
            while (watch.ElapsedMilliseconds < MinimumSplashDelay)
            { await Task.Delay(100); }

            // Switch Page
            IoC.Get<MainWindow>().Dispatcher.Invoke(() =>
            {
                var viewModel = IoC.Get<WindowViewModel>();
                viewModel.CurrentPage = Page.Base;
            });
        }

        /// <summary>
        /// Runs a method intended to update the UI thread.
        /// </summary>
        private void UpdateText(string newText)
        {
            this.Dispatcher.Invoke(() => { _splashViewModel.Text = newText; });
        }

        /// <summary>
        /// Creates a new configuration if the config does not exist.
        /// </summary>
        private LoaderConfig CreateNewConfigIfNotExist()
        {
            var configReader = new LoaderConfigReader();
            if (!configReader.ConfigurationExists())
                configReader.WriteConfiguration(new LoaderConfig());

            return configReader.ReadConfiguration();
        }
    }
}
