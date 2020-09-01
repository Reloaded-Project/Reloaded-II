﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Win32;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Dependency;
using Reloaded.Mod.Loader.Update.Resolvers;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.Mod.Shared;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Contains all code for setting up the running of Reloaded at startup.
    /// </summary>
    public static class Setup
    {
        private static bool _loadExecuted = false;
        private static XamlResource<string> _xamlSplashCreatingDefaultConfig = new XamlResource<string>("SplashCreatingDefaultConfig");
        private static XamlResource<string> _xamlSplashPreparingResources = new XamlResource<string>("SplashPreparingResources");
        private static XamlResource<string> _xamlCheckingForUpdates = new XamlResource<string>("SplashCheckingForUpdates");
        private static XamlResource<string> _xamlSplashLoadCompleteIn = new XamlResource<string>("SplashLoadCompleteIn");
        private static XamlResource<string> _xamlCreatingTemplates = new XamlResource<string>("SplashCreatingTemplates");
        private static XamlResource<string> _xamlRunningSanityChecks = new XamlResource<string>("SplashRunningSanityChecks");

        /// <summary>
        /// Sets up the overall application state for either running or testing.
        /// Note: This method uses Task.Delay for waiting the minimum splash delay without blocking the thread, it is synchronous.
        /// </summary>
        /// <param name="updateText">A function that updates the visible text onscreen.</param>
        /// <param name="minimumSplashDelay">Minimum amount of time to wait to complete the loading process.</param>
        public static async Task SetupApplicationAsync(Action<string> updateText, int minimumSplashDelay)
        {
            if (!_loadExecuted)
            {
                // Benchmark load time.
                _loadExecuted = true;
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Allow for debugging before crashing.
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                RegisterReloadedProtocol();
                CheckForMissingDependencies();

                updateText(_xamlSplashCreatingDefaultConfig.Get());
                CreateNewConfigIfNotExist();

                updateText(_xamlCreatingTemplates.Get());
                CreateTemplates();

                updateText(_xamlSplashPreparingResources.Get());
                await SetupViewModelsAsync();

                updateText(_xamlCheckingForUpdates.Get());
                #pragma warning disable 4014
                CheckForUpdatesAsync(); // Fire and forget, we don't want to delay startup time.
                #pragma warning restore 4014

                updateText(_xamlRunningSanityChecks.Get());
                DoSanityTests();

                // Wait until splash screen time.
                updateText($"{_xamlSplashLoadCompleteIn.Get()} {watch.ElapsedMilliseconds}ms");
                
                while (watch.ElapsedMilliseconds < minimumSplashDelay)
                {
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Checks for missing mod loader dependencies.
        /// </summary>
        private static void CheckForMissingDependencies()
        {
            var deps = new DependencyChecker();
            if (!deps.AllAvailable)
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                {
                    var dialog = new MissingCoreDependencyDialog(deps);
                    dialog.ShowDialog();
                });
            }
        }

        /// <summary>
        /// Tries to find all incompatible mods to the current application config's set of mods.
        /// Sanity check after loading mod sets.
        /// </summary>
        /// <param name="incompatibleModIds">List of incompatible mods.</param>
        public static bool TryGetIncompatibleMods(ImageApplicationPathTuple application, IEnumerable<ImageModPathTuple> mods, out List<ImageModPathTuple> incompatibleModIds)
        {
            var enabledModIds = application.Config.EnabledMods;
            incompatibleModIds = new List<ImageModPathTuple>(enabledModIds.Length);

            foreach (var enabledModId in enabledModIds)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                var mod = mods.FirstOrDefault(x => x.ModConfig.ModId == enabledModId);
                if (mod == null)
                    continue;

                if (!mod.ModConfig.SupportedAppId.Contains(application.Config.AppId))
                    incompatibleModIds.Add(mod);
            }

            return incompatibleModIds.Count > 0;
        }

        /// <summary>
        /// Quickly checks for any missing dependencies amongst available mods
        /// and opens a menu allowing to download if mods are unavailable.
        /// </summary>
        /// <returns></returns>
        public static async Task CheckForMissingModDependencies()
        {
            if (Update.CheckMissingDependencies(out var missingDependencies))
            {
                try
                {
                    await Update.DownloadPackagesAsync(missingDependencies, false, false);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Tests possible error cases.
        /// </summary>
        private static void DoSanityTests()
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                // Needs to be ran after SetupViewModelsAsync
                var apps = IoC.GetConstant<MainPageViewModel>().Applications;
                var mods = IoC.GetConstant<ManageModsViewModel>().Mods;

                foreach (var app in apps)
                {
                    if (TryGetIncompatibleMods(app, mods, out var incompatible))
                        new IncompatibleModDialog(incompatible, app).ShowDialog();
                }
            });
        }

        private static void RegisterReloadedProtocol()
        {            
            // Get the user classes subkey.
            var classesSubKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

            // Add a Reloaded Key.
            RegistryKey reloadedProtocolKey = classesSubKey?.CreateSubKey($"{Constants.ReloadedProtocol}");
            reloadedProtocolKey?.SetValue("", $"URL:{Constants.ReloadedProtocol}");
            reloadedProtocolKey?.SetValue("URL Protocol", "");
            reloadedProtocolKey?.CreateSubKey(@"shell\open\command")?.SetValue("", $"\"{Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe")}\" {Constants.ParameterDownload} %1");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e) => Errors.HandleException((Exception) e.ExceptionObject);

        /// <summary>
        /// Creates a new configuration if the config does not exist.
        /// </summary>
        private static void CreateNewConfigIfNotExist()
        {
            if (!LoaderConfigReader.ConfigurationExists())
                LoaderConfigReader.WriteConfiguration(new LoaderConfig());
        }

        /// <summary>
        /// Sets up viewmodels to be used in the individual mod loader pages.
        /// </summary>
        private static async Task SetupViewModelsAsync()
        {
            Task.Run(BasicDllInjector.PreloadAddresses); // Fire and Forget

            LoaderConfig config;
            try
            {
                config = LoaderConfigReader.ReadConfiguration();
            }
            catch (Exception ex)
            {
                config = new LoaderConfig();
                config.SanitizeConfig();
                LoaderConfigReader.WriteConfiguration(config);
                Errors.HandleException(ex, "Failed to parse Reloaded-II launcher configuration.\n" +
                                           "This is a rare bug, your settings have been reset.\n" +
                                           "If you have encountered this please report this to the GitHub issue tracker.\n" +
                                           "Any information on how to reproduce this would be very, very welcome.\n");
            }

            IoC.Kernel.Bind<LoaderConfig>().ToConstant(config);
            IoC.GetConstant<MainPageViewModel>();
            IoC.GetConstant<ManageModsViewModel>();   // Consumes MainPageViewModel, LoaderConfig
            IoC.GetConstant<SettingsPageViewModel>(); // Consumes ManageModsViewModel, MainPageViewModel

            try
            {
                var helper = await NugetHelper.FromSourceUrlAsync(SharedConstants.NuGetApiEndpoint);
                IoC.Kernel.Rebind<NugetHelper>().ToConstant(helper);
                IoC.GetConstant<DownloadModsViewModel>(); // Consumes ManageModsViewModel, NugetHelper
            }
            catch (Exception)
            {
                // Probably no internet access. 
            }

            /* Set loader DLL path. */
            SetLoaderPaths(config, Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));
            LoaderConfigReader.WriteConfiguration(config);
        }

        /// <summary>
        /// Sets the Reloaded Mod Loader DLL paths for a launcher config.
        /// </summary>
        private static void SetLoaderPaths(LoaderConfig config, string launcherDirectory)
        {
            if (String.IsNullOrEmpty(launcherDirectory))
                throw new DllNotFoundException("The provided launcher directory is null or empty. This is a bug. Report this to the developer.");

            // Loader configuration.
            var loaderPath32 = Path.Combine(launcherDirectory, $"Loader\\x86\\{LoaderConfig.LoaderDllName}");
            if (! File.Exists(loaderPath32))
                throw new DllNotFoundException($"(x86) {LoaderConfig.LoaderDllName} {Errors.LoaderNotFound()}");

            var loaderPath64 = Path.Combine(launcherDirectory, $"Loader\\x64\\{LoaderConfig.LoaderDllName}");
            if (!File.Exists(loaderPath64))
                throw new DllNotFoundException($"(x64) {LoaderConfig.LoaderDllName} {Errors.LoaderNotFound()}");

            // Bootstrappers.
            var bootstrapper32Path = Path.Combine(launcherDirectory, $"Loader\\X86\\Bootstrapper\\{LoaderConfig.Bootstrapper32Name}");
            if (!File.Exists(bootstrapper32Path))
                throw new DllNotFoundException($"{LoaderConfig.Bootstrapper32Name} {Errors.LoaderNotFound()}");

            var bootstrapper64Path = Path.Combine(launcherDirectory, $"Loader\\X64\\Bootstrapper\\{LoaderConfig.Bootstrapper64Name}");
            if (!File.Exists(bootstrapper64Path))
                throw new DllNotFoundException($"{LoaderConfig.Bootstrapper64Name} {Errors.LoaderNotFound()}");

            // Set to config.
            config.LauncherPath = Process.GetCurrentProcess().MainModule.FileName;
            config.LoaderPath32 = loaderPath32;
            config.LoaderPath64 = loaderPath64;
            config.Bootstrapper32Path = bootstrapper32Path;
            config.Bootstrapper64Path = bootstrapper64Path;
        }

        /// <summary>
        /// Creates templates for configurations not available in the GUI.
        /// </summary>
        private static void CreateTemplates()
        {
            var templatesDirectory = Path.GetFullPath("Templates");
            if (!Directory.Exists(templatesDirectory))
                Directory.CreateDirectory(templatesDirectory);
            
            GitHubLatestUpdateResolver.GitHubConfig.ToPath(new GitHubLatestUpdateResolver.GitHubConfig(), $"{GitHubLatestUpdateResolver.GitHubConfig.GetFilePath(templatesDirectory)}");
            GameBananaUpdateResolver.GameBananaConfig.ToPath(new GameBananaUpdateResolver.GameBananaConfig(), $"{GameBananaUpdateResolver.GameBananaConfig.GetFilePath(templatesDirectory)}");
        }

        /// <summary>
        /// Checks for mod loader updates.
        /// </summary>
        private static async Task CheckForUpdatesAsync()
        {
            await Task.Run(Update.CheckForModUpdatesAsync);
            await Update.CheckForLoaderUpdatesAsync();
            await CheckForMissingModDependencies();
        }

    }
}
