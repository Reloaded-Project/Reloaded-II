using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Contains all code for setting up the running of Reloaded at startup.
    /// </summary>
    public static class Setup
    {
        #region XAML String Resource Constants
        // ReSharper disable InconsistentNaming
        private const string XAML_SplashCreatingDefaultConfig = "SplashCreatingDefaultConfig";
        private const string XAML_SplashCleaningConfigurations = "SplashCleaningConfigurations";
        private const string XAML_SplashPreparingViewModels = "SplashPreparingViewModels";
        private const string XAML_SplashLoadCompleteIn = "SplashLoadCompleteIn";
        // ReSharper restore InconsistentNaming
        #endregion XAML String Resource Constants

        private static bool _loadExecuted = false;

        /// <summary>
        /// Sets up the overall application state for either running or testing.
        /// Note: This method uses Task.Delay for waiting the minimum splash delay without blocking the thread, it is synchronous.
        /// </summary>
        /// <param name="getText">A function that accepts the name of a XAML resource and retrieves the text to update with.</param>
        /// <param name="updateText">A function that updates the visible text onscreen.</param>
        /// <param name="minimumSplashDelay">Minimum amount of time to wait to complete the loading process.</param>
        public static async Task SetupApplication(Func<string, string> getText, Action<string> updateText, int minimumSplashDelay = 1000)
        {
            if (!_loadExecuted)
            {
                // Benchmark load time.
                _loadExecuted = true;
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Setting up localization.
                SetupLocalization();

                // Make Default Config if Necessary.
                updateText(getText(XAML_SplashCreatingDefaultConfig));
                var _loaderConfig = CreateNewConfigIfNotExist();

                // Cleaning up App/Loader/Mod Configurations
                updateText(getText(XAML_SplashCleaningConfigurations));
                CleanupConfigurations(_loaderConfig);

                // Preparing viewmodels.
                updateText(getText(XAML_SplashPreparingViewModels));
                SetupViewModels();

                // Wait until splash screen time.
                updateText($"{getText(XAML_SplashLoadCompleteIn)} {watch.ElapsedMilliseconds}ms");
                
                while (watch.ElapsedMilliseconds < minimumSplashDelay)
                {
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Sets up localization for the system language.
        /// </summary>
        private static void SetupLocalization()
        {
            // Set language dictionary.
            var langDict = new ResourceDictionary();
            string culture = Thread.CurrentThread.CurrentCulture.ToString();
            string languageFilePath = AppDomain.CurrentDomain.BaseDirectory + $"/Languages/{culture}.xaml";
            if (File.Exists(languageFilePath))
                langDict.Source = new Uri(languageFilePath, UriKind.Absolute);

            Application.Current.Resources.MergedDictionaries.Add(langDict);
        }

        /// <summary>
        /// Creates a new configuration if the config does not exist.
        /// </summary>
        private static LoaderConfig CreateNewConfigIfNotExist()
        {
            var configReader = new LoaderConfigReader();
            if (!configReader.ConfigurationExists())
                configReader.WriteConfiguration(new LoaderConfig());

            return configReader.ReadConfiguration();
        }

        /// <summary>
        /// Sets up viewmodels to be used in the individual mod loader pages.
        /// </summary>
        private static void SetupViewModels()
        {
            var mainPageViewModel = IoC.Get<MainPageViewModel>();
            IoC.Kernel.Bind<MainPageViewModel>().ToConstant(mainPageViewModel);

            // Consumes MainPageViewModel, make sure it goes after it.
            var addAppViewModel = IoC.Get<AddAppViewModel>();
            IoC.Kernel.Bind<AddAppViewModel>().ToConstant(addAppViewModel);
        }


        /// <summary>
        /// Cleans up App/Loader/Mod Configurations from nonexisting
        /// references such as removed mods.
        /// </summary>
        private static void CleanupConfigurations(LoaderConfig loaderConfig)
        {
            var modConfigLoader = new ConfigLoader<ModConfig>();
            var appConfigLoader = new ConfigLoader<ApplicationConfig>();
            var loaderConfigReader = new LoaderConfigReader();

            foreach (var modConfiguration in modConfigLoader.ReadConfigurations(loaderConfig.ModConfigDirectory, ModConfig.ConfigFileName))
            {
                modConfiguration.Object.CleanupConfig(modConfiguration.Path);
                modConfigLoader.WriteConfiguration(modConfiguration.Path, modConfiguration.Object);
            }

            foreach (var appConfiguration in appConfigLoader.ReadConfigurations(loaderConfig.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName))
            {
                appConfiguration.Object.CleanupConfig(appConfiguration.Path);
                appConfigLoader.WriteConfiguration(appConfiguration.Path, appConfiguration.Object);
            }

            var loaderConfiguration = loaderConfigReader.ReadConfiguration();
            loaderConfiguration.CleanupConfig(loaderConfigReader.ConfigurationPath());
            loaderConfigReader.WriteConfiguration(loaderConfiguration);
        }
    }
}
