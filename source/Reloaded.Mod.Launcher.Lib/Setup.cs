using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Reloaded.Mod.Launcher.Lib.Misc;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Launcher.Lib.Utility.Interfaces;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Dependency;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Contains all code for setting up the running of Reloaded at startup.
/// </summary>
public static class Setup
{
    private static bool _loadExecuted = false;

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

            updateText(Resources.SplashCreatingDefaultConfig.Get());
            await UpdateDefaultConfig();
            CheckForMissingDependencies();

            updateText(Resources.SplashPreparingResources.Get());
            await Task.Run(SetupServices);
            await Task.Run(SetupViewModels);

            updateText(Resources.SplashCheckingForUpdates.Get());
#pragma warning disable 4014
            CheckForUpdatesAsync(); // Fire and forget, we don't want to delay startup time.
#pragma warning restore 4014

            updateText(Resources.SplashRunningSanityChecks.Get());
            DoSanityTests();
            var _ = Task.Run(CompressOldLogs);

            // Wait until splash screen time.
            updateText($"{Resources.SplashLoadCompleteIn.Get()} {watch.ElapsedMilliseconds}ms");
                
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
    /// Checks for missing mod loader dependencies.
    /// </summary>
    private static void CheckForMissingDependencies()
    {
        var deps = new DependencyChecker(IoC.Get<LoaderConfig>(), IntPtr.Size == 8);
        if (deps.AllAvailable) 
            return;

        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
        {
            Actions.ShowMissingCoreDependencyDialog(new MissingCoreDependencyDialogViewModel(deps));
        });
    }

    /// <summary>
    /// Tries to find all incompatible mods to the current application config's set of mods.
    /// Sanity check after loading mod sets.
    /// </summary>
    /// <param name="mods">The mods to check compatibility for.</param>
    /// <param name="incompatibleMods">List of incompatible mods.</param>
    /// <param name="application">The application for which to check compatibility.</param>
    public static bool TryGetIncompatibleMods(PathTuple<ApplicationConfig> application, IEnumerable<PathTuple<ModConfig>> mods, out List<PathTuple<ModConfig>> incompatibleMods)
    {
        var enabledModIds = application.Config.EnabledMods;
        incompatibleMods = new List<PathTuple<ModConfig>>(enabledModIds.Length);

        foreach (var enabledModId in enabledModIds)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var mod = mods.FirstOrDefault(x => x.Config.ModId == enabledModId);
            if (mod == null)
                continue;

            if (!mod.Config.SupportedAppId.Contains(application.Config.AppId))
                incompatibleMods.Add(mod);
        }

        return incompatibleMods.Count > 0;
    }

    /// <summary>
    /// Quickly checks for any missing dependencies amongst available mods
    /// and opens a menu allowing to download if mods are unavailable.
    /// </summary>
    /// <returns></returns>
    public static async Task CheckForMissingModDependenciesAsync()
    {
        var deps = Update.CheckMissingDependencies();
        if (!deps.AllAvailable)
        {
            try
            {
                await Update.ResolveMissingPackagesAsync(deps);
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
            var apps = IoC.GetConstant<ApplicationConfigService>().Items;
            var mods = IoC.GetConstant<ModConfigService>().Items;
            var updatedBootstrappers = new List<string>();

            foreach (var app in apps)
            {
                // Incompatible Mods
                EnforceModCompatibility(app, mods);

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
                    catch (Exception) { /* ignored */ }

                    var bootstrapperSourcePath = deployer.GetBootstrapperDllPath();
                    try
                    {
                        File.Copy(bootstrapperSourcePath, bootstrapperInstallPath, true);
                        updatedBootstrappers.Add(app.Config.AppName);
                    }
                    catch (Exception) { /* ignored */  }
                }
                catch (Exception) { /* ignored */ }
            }

            if (updatedBootstrappers.Count <= 0) 
                return;

            var title = Resources.BootstrapperUpdateTitle.Get();
            var description = string.Format(Resources.BootstrapperUpdateDescription.Get(), String.Join('\n', updatedBootstrappers));
            Actions.DisplayMessagebox.Invoke(title, description);
        });
    }

    private static void RegisterReloadedProtocol()
    {            
        // Get the user classes subkey.
        var classesSubKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

        // Add a Reloaded Key.
        RegistryKey reloadedProtocolKey = classesSubKey?.CreateSubKey($"{Constants.ReloadedProtocol}")!;
        reloadedProtocolKey?.SetValue("", $"URL:{Constants.ReloadedProtocol}");
        reloadedProtocolKey?.SetValue("URL Protocol", "");
        reloadedProtocolKey?.CreateSubKey(@"shell\open\command")?.SetValue("", $"\"{Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe")}\" {Constants.ParameterDownload} %1");
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
        SynchronizationContext? synchronizationContext = null;
        ActionWrappers.ExecuteWithApplicationDispatcher(() => synchronizationContext = SynchronizationContext.Current);

        IoC.Kernel.Rebind<IProcessWatcher>().ToConstant(IProcessWatcher.Get());
        IoC.Kernel.Rebind<ApplicationConfigService>().ToConstant(new ApplicationConfigService(config, synchronizationContext));

        var modConfigService = new ModConfigService(config, synchronizationContext);
        IoC.Kernel.Rebind<ModConfigService>().ToConstant(modConfigService);
        IoC.Kernel.Rebind<ModUserConfigService>().ToConstant(new ModUserConfigService(config, modConfigService, synchronizationContext));
    }

    /// <summary>
    /// Sets up viewmodels to be used in the individual mod loader pages.
    /// </summary>
    private static void SetupViewModels()
    {
        var config = IoC.Get<LoaderConfig>();
        IoC.GetConstant<MainPageViewModel>();

        try
        {
            IoC.Kernel.Rebind<AggregateNugetRepository>().ToConstant(new AggregateNugetRepository(config.NuGetFeeds));
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
            throw new DllNotFoundException($"(x86) {Path.GetFileName(loaderPath32)} {Resources.ErrorLoaderNotFound.Get()}");

        var loaderPath64 = Paths.GetLoaderPath64(launcherDirectory);
        if (!File.Exists(loaderPath64))
            throw new DllNotFoundException($"(x64) {Path.GetFileName(loaderPath64)} {Resources.ErrorLoaderNotFound.Get()}");

        // Bootstrappers.
        var bootstrapper32Path = Paths.GetBootstrapperPath32(launcherDirectory);
        if (!File.Exists(bootstrapper32Path))
            throw new DllNotFoundException($"{Path.GetFileName(bootstrapper32Path)} {Resources.ErrorLoaderNotFound.Get()}");

        var bootstrapper64Path = Paths.GetBootstrapperPath64(launcherDirectory);
        if (!File.Exists(bootstrapper64Path))
            throw new DllNotFoundException($"{Path.GetFileName(bootstrapper64Path)} {Resources.ErrorLoaderNotFound.Get()}");

        // Set to config.
        config.LauncherPath = Process.GetCurrentProcess().MainModule!.FileName;
        config.LoaderPath32 = loaderPath32;
        config.LoaderPath64 = loaderPath64;
        config.Bootstrapper32Path = bootstrapper32Path;
        config.Bootstrapper64Path = bootstrapper64Path;

        // Update Environment Variables
        Environment.SetEnvironmentVariable("RELOADEDIIMODS", config.ModConfigDirectory, EnvironmentVariableTarget.User);
    }

    /// <summary>
    /// Checks for mod loader updates.
    /// </summary>
    private static async Task CheckForUpdatesAsync()
    {
        await DependencyMetadataWriterFactory.ExecuteAllAsync(IoC.Get<ModConfigService>());
        await Task.Run(Update.CheckForModUpdatesAsync);
        await Update.CheckForLoaderUpdatesAsync();
        await CheckForMissingModDependenciesAsync();
    }

    /// <summary>
    /// Enforces that all mods are compatible with a given application.
    /// </summary>
    /// <param name="applicationTuple">The application to enforce all mods are compatible for.</param>
    /// <param name="items">The mods to ensure are compatible.</param>
    public static void EnforceModCompatibility(PathTuple<ApplicationConfig> applicationTuple, IEnumerable<PathTuple<ModConfig>> items)
    {
        if (!TryGetIncompatibleMods(applicationTuple, items, out var incompatibleMods))
            return;

        foreach (var modId in incompatibleMods)
        {
            var supportedAppIds = new List<string>(modId.Config.SupportedAppId) { applicationTuple.Config.AppId };
            modId.Config.SupportedAppId = supportedAppIds.ToArray();
            modId.Save();
        }
    }
}