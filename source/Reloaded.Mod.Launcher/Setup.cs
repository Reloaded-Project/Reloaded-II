using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Launcher.Utility.Interfaces;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Dependency;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.WPF.Utilities;
using MessageBox = System.Windows.MessageBox;

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
        private static XamlResource<string> _xamlRunningSanityChecks = new XamlResource<string>("SplashRunningSanityChecks");

        private static XamlResource<string> _xamlBootstrapperUpdateTitle = new XamlResource<string>("BootstrapperUpdateTitle");
        private static XamlResource<string> _xamlBootstrapperUpdateDescription = new XamlResource<string>("BootstrapperUpdateDescription");

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

                updateText(_xamlSplashCreatingDefaultConfig.Get());
                await UpdateDefaultConfig();
                CheckForMissingDependencies();

                updateText(_xamlSplashPreparingResources.Get());
                await Task.Run(SetupServices);
                await Task.Run(SetupViewModels);

                updateText(_xamlCheckingForUpdates.Get());
                #pragma warning disable 4014
                CheckForUpdatesAsync(); // Fire and forget, we don't want to delay startup time.
                #pragma warning restore 4014

                updateText(_xamlRunningSanityChecks.Get());
                DoSanityTests();
                CleanupAfterOldVersions();
                var _ = Task.Run(CompressOldLogs);

                // Wait until splash screen time.
                updateText($"{_xamlSplashLoadCompleteIn.Get()} {watch.ElapsedMilliseconds}ms");
                
                while (watch.ElapsedMilliseconds < minimumSplashDelay)
                {
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Compresses old loader log files into a zip archive.
        /// </summary>
        private static void CompressOldLogs()
        {
            using var logCompressor = new LogFileCompressor(Paths.ArchivedLogPath);
            var loaderConfig = IoC.Get<LoaderConfig>();
            logCompressor.AddFiles(Paths.LogPath, TimeSpan.FromHours(loaderConfig.LogFileCompressTimeHours));
            logCompressor.DeleteOldFiles(TimeSpan.FromHours(loaderConfig.LogFileDeleteHours));
        }

        /// <summary>
        /// Cleans up files/folders after upgrades from old loader versions.
        /// </summary>
        private static void CleanupAfterOldVersions()
        {
            var launcherFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            DestroyFolder(launcherFolder, "Languages");
            DestroyFolder(launcherFolder, "Styles");
            DestroyFolder(launcherFolder, "Templates");

            #if !DEBUG
            DestroyFileType(launcherFolder, "*.pdb");
            DestroyFileType(launcherFolder, "*.xml");
            #endif

            // Check for .NET 5 Single File
            // This code is unused until .NET 5 upgrade.
            if (Environment.Version >= Version.Parse("5.0.0"))
            {
                // Currently no API to check if in bundle (single file app); however, we know that CodeBase returns true when loaded from memory.
                var resAsm = Application.ResourceAssembly;
                var configuration = resAsm.GetCustomAttribute<AssemblyConfigurationAttribute>();
                if (configuration != null && configuration.Configuration.Contains("SingleFile"))
                {
                    // TODO: Needs more testing. Seems to work but might be problematic.
                    DestroyFileType(launcherFolder, "*.json");
                    DestroyFileType(launcherFolder, "*.dll");
                }
            }

            void DestroyFolder(string baseFolder, string folderName)
            {
                var folderPath = Path.Combine(baseFolder, folderName);
                ActionWrappers.TryCatchDiscard(() => Directory.Delete(folderPath, true));
            }

            void DestroyFileType(string baseFolder, string searchPattern)
            {
                var files = Directory.GetFiles(baseFolder, searchPattern);
                foreach (var file in files)
                    ActionWrappers.TryCatchDiscard(() => File.Delete(file));
            }
        }

        /// <summary>
        /// Checks for missing mod loader dependencies.
        /// </summary>
        private static void CheckForMissingDependencies()
        {
            var deps = new DependencyChecker(IoC.Get<LoaderConfig>(), IntPtr.Size == 8);
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
        public static bool TryGetIncompatibleMods(PathTuple<ApplicationConfig> application, ObservableCollection<PathTuple<ModConfig>> mods, out List<PathTuple<ModConfig>> incompatibleModIds)
        {
            var enabledModIds = application.Config.EnabledMods;
            incompatibleModIds = new List<PathTuple<ModConfig>>(enabledModIds.Length);

            foreach (var enabledModId in enabledModIds)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                var mod = mods.FirstOrDefault(x => x.Config.ModId == enabledModId);
                if (mod == null)
                    continue;

                if (!mod.Config.SupportedAppId.Contains(application.Config.AppId))
                    incompatibleModIds.Add(mod);
            }

            return incompatibleModIds.Count > 0;
        }

        /// <summary>
        /// Quickly checks for any missing dependencies amongst available mods
        /// and opens a menu allowing to download if mods are unavailable.
        /// </summary>
        /// <returns></returns>
        public static async Task CheckForMissingModDependenciesAsync()
        {
            if (Update.CheckMissingDependencies(out var missingDependencies))
            {
                try
                {
                    await Update.DownloadNuGetPackagesAsync(missingDependencies, false, false);
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
                var apps = IoC.GetConstant<ApplicationConfigService>().Applications;
                var mods = IoC.GetConstant<ModConfigService>().Mods;
                var updatedBootstrappers = new List<string>();

                foreach (var app in apps)
                {
                    // Incompatible Mods
                    if (TryGetIncompatibleMods(app, mods, out var incompatible))
                        new IncompatibleModDialog(incompatible, app).ShowDialog();

                    // Bootstrapper Update
                    try
                    {
                        var deployer = new AsiLoaderDeployer(app);
                        var bootstrapperInstallPath = deployer.GetBootstrapperInstallPath(out _);
                        if (!File.Exists(bootstrapperInstallPath))
                            continue;

                        var updater = new BootstrapperUpdateChecker(bootstrapperInstallPath);
                        try
                        {
                            if (!updater.NeedsUpdate())
                                continue;
                        }
                        catch (Exception e) { /* ignored */ }

                        var bootstrapperSourcePath = deployer.GetBootstrapperDllPath();
                        try
                        {
                            File.Copy(bootstrapperSourcePath, bootstrapperInstallPath, true);
                            updatedBootstrappers.Add(app.Config.AppName);
                        }
                        catch (Exception e) { /* ignored */  }
                    }
                    catch (Exception e) { /* ignored */ }
                }

                if (updatedBootstrappers.Count > 0)
                {
                    var messageBox = new Pages.Dialogs.MessageBox
                    (
                        _xamlBootstrapperUpdateTitle.Get(), 
                        string.Format(_xamlBootstrapperUpdateDescription.Get(), String.Join('\n', updatedBootstrappers))
                    );

                    messageBox.ShowDialog();
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
        /// Updates the default config.
        /// </summary>
        private static async Task UpdateDefaultConfig()
        {
            var config = IoC.Get<LoaderConfig>();
            SetLoaderPaths(config, Paths.CurrentProgramFolder);
            await IConfig<LoaderConfig>.ToPathAsync(config, Paths.LoaderConfigPath);
        }

        /// <summary>
        /// Sets up services which can be used by the various viewmodels.
        /// </summary>
        private static void SetupServices()
        {
            var config = IoC.Get<LoaderConfig>();
            SynchronizationContext synchronizationContext = null;
            ActionWrappers.ExecuteWithApplicationDispatcher(() => synchronizationContext = SynchronizationContext.Current);

            IoC.Kernel.Rebind<IProcessWatcher>().ToConstant(IProcessWatcher.Get());
            IoC.Kernel.Rebind<ApplicationConfigService>().ToConstant(new ApplicationConfigService(config, synchronizationContext));
            IoC.Kernel.Rebind<ModConfigService>().ToConstant(new ModConfigService(config, synchronizationContext));
        }

        /// <summary>
        /// Sets up viewmodels to be used in the individual mod loader pages.
        /// </summary>
        private static void SetupViewModels()
        {
            var config = IoC.Get<LoaderConfig>();
            IoC.GetConstant<MainPageViewModel>();
            IoC.GetConstant<ManageModsViewModel>();  
            IoC.GetConstant<SettingsPageViewModel>(); 

            try
            {
                IoC.Kernel.Rebind<AggregateNugetRepository>().ToConstant(new AggregateNugetRepository(config.NuGetFeeds));
                IoC.GetConstant<DownloadModsViewModel>(); // Consumes ManageModsViewModel, NugetHelper
            }
            catch (Exception)
            {
                // Probably no internet access. 
            }
        }

        /// <summary>
        /// Sets the Reloaded Mod Loader DLL paths for a launcher config.
        /// </summary>
        private static void SetLoaderPaths(LoaderConfig config, string launcherDirectory)
        {
            if (String.IsNullOrEmpty(launcherDirectory))
                throw new DllNotFoundException("The provided launcher directory is null or empty. This is a bug. Report this to the developer.");

            // Loader configuration.
            var loaderPath32 = Paths.GetLoaderPath32(launcherDirectory);
            if (! File.Exists(loaderPath32))
                throw new DllNotFoundException($"(x86) {Path.GetFileName(loaderPath32)} {Errors.LoaderNotFound()}");

            var loaderPath64 = Paths.GetLoaderPath64(launcherDirectory);
            if (!File.Exists(loaderPath64))
                throw new DllNotFoundException($"(x64) {Path.GetFileName(loaderPath64)} {Errors.LoaderNotFound()}");

            // Bootstrappers.
            var bootstrapper32Path = Paths.GetBootstrapperPath32(launcherDirectory);
            if (!File.Exists(bootstrapper32Path))
                throw new DllNotFoundException($"{Path.GetFileName(bootstrapper32Path)} {Errors.LoaderNotFound()}");

            var bootstrapper64Path = Paths.GetBootstrapperPath64(launcherDirectory);
            if (!File.Exists(bootstrapper64Path))
                throw new DllNotFoundException($"{Path.GetFileName(bootstrapper64Path)} {Errors.LoaderNotFound()}");

            // Set to config.
            config.LauncherPath = Process.GetCurrentProcess().MainModule.FileName;
            config.LoaderPath32 = loaderPath32;
            config.LoaderPath64 = loaderPath64;
            config.Bootstrapper32Path = bootstrapper32Path;
            config.Bootstrapper64Path = bootstrapper64Path;
        }

        /// <summary>
        /// Checks for mod loader updates.
        /// </summary>
        private static async Task CheckForUpdatesAsync()
        {
            await Task.Run(Update.CheckForModUpdatesAsync);
            await Update.CheckForLoaderUpdatesAsync();
            await CheckForMissingModDependenciesAsync();
        }
    }
}
