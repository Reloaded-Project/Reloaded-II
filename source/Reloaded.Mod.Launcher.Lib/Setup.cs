using Application = System.Windows.Application;
using Environment = System.Environment;

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
    /// <param name="backgroundTasks">Contains all background tasks.</param>
    public static async Task SetupApplicationAsync(Action<string> updateText, int minimumSplashDelay, List<Task> backgroundTasks)
    {
        if (!_loadExecuted)
        {
            // Benchmark load time.
            _loadExecuted = true;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Allow for debugging before crashing.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            var setupServicesTask = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, SetupServices);
            RegisterReloadedProtocol();
            RegisterR2PackProtocol();
            RegisterR2Pack();

            updateText(Resources.SplashCreatingDefaultConfig.Get());
            backgroundTasks.Add(UpdateDefaultConfig()); // Fire and forget.
            CheckForMissingDependencies();
                
            updateText(Resources.SplashPreparingResources.Get());
            backgroundTasks.Add(Task.Run(CheckForUpdatesAsync)); // Fire and forget, we don't want to delay startup time.
            await setupServicesTask; // required for viewmodels & sanity tests.
            SetupViewModels();

            updateText(Resources.SplashRunningSanityChecks.Get());
            backgroundTasks.Add(DoSanityTests());
            backgroundTasks.Add(Task.Run(HandleLogsAndCrashdumps));

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
    private static void HandleLogsAndCrashdumps()
    {
        var loaderConfig = IoC.Get<LoaderConfig>();

        // Logs (delete & compress)
        using var logCompressor = new LogFileCompressor(Paths.ArchivedLogPath);
        logCompressor.AddFiles(Paths.LogPath, TimeSpan.FromHours(loaderConfig.LogFileCompressTimeHours));
        logCompressor.DeleteOldFiles(TimeSpan.FromHours(loaderConfig.LogFileDeleteHours));

        // Crashdumps (delete)
        Directory.CreateDirectory(Paths.CrashDumpPath);
        var dumpFolders = Directory.GetDirectories(Paths.CrashDumpPath);
        var now = DateTime.UtcNow;
        foreach (var folder in dumpFolders)
        {
            var timeElapsed = now - Directory.GetLastWriteTimeUtc(folder);
            if (timeElapsed.TotalHours > loaderConfig.CrashDumpDeleteHours)
                IOEx.TryDeleteDirectory(folder);
        }
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
    /// <returns>True if there are missing deps, else false.</returns>
    public static async Task CheckForMissingModDependenciesAsync()
    {
        var deps = Update.CheckMissingDependencies();
        if (!deps.AllAvailable)
        {
            try
            {
                await Update.ResolveMissingPackagesAsync();
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
    private static Task DoSanityTests()
    {
        // Needs to be ran after SetupViewModelsAsync
        var apps = IoC.GetConstant<ApplicationConfigService>().Items;
        var mods = IoC.GetConstant<ModConfigService>().Items.ToArray();
        
        // Unprotect all MS Store titles if needed.
        foreach (var app in apps)
        {
            if (app.Config.IsMsStore)
            {
                TryUnprotectGamePassGame.TryIgnoringErrors(app.Config.AppLocation);
            }
        }
        
        // Enforce compatibility non-async, since this is unlikely to do anything.
        foreach (var app in apps)
            EnforceModCompatibility(app, mods);

        // Checking bootstrappers could add I/O overhead on cold boot, delegate to threadpool.
        void UpdateBootstrappers()
        {
            var updatedBootstrappers = new List<string>();
            foreach (var app in apps)
            {
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
                    catch (Exception) { /* ignored */ }
                }
                catch (Exception) { /* ignored */ }
            }

            if (updatedBootstrappers.Count <= 0) 
                return;

            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                var title = Resources.BootstrapperUpdateTitle.Get();
                var description = string.Format(Resources.BootstrapperUpdateDescription.Get(), String.Join('\n', updatedBootstrappers));
                Actions.DisplayMessagebox.Invoke(title, description);
            });
        }

        return Task.Run(UpdateBootstrappers);
    }

    private static void RegisterReloadedProtocol()
    {            
        // Get the user classes subkey.
        var classesSubKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

        // Add a Reloaded Key.
        var reloadedProtocolKey = classesSubKey?.CreateSubKey($"{Constants.ReloadedProtocol}")!;
        reloadedProtocolKey?.SetValue("", $"URL:{Constants.ReloadedProtocol}");
        reloadedProtocolKey?.SetValue("URL Protocol", "");
        reloadedProtocolKey?.CreateSubKey(@"shell\open\command")?.SetValue("", $"\"{Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe")}\" {Constants.ParameterDownload} %1");
    }

    private static void RegisterR2PackProtocol()
    {
        // Get the user classes subkey.
        var classesSubKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

        // Add a Reloaded Key.
        var reloadedProtocolKey = classesSubKey?.CreateSubKey($"{Constants.ReloadedPackProtocol}")!;
        reloadedProtocolKey?.SetValue("", $"URL:{Constants.ReloadedPackProtocol}");
        reloadedProtocolKey?.SetValue("URL Protocol", "");
        reloadedProtocolKey?.CreateSubKey(@"shell\open\command")?.SetValue("", $"\"{Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe")}\" {Constants.ParameterR2PackDownload} %1");
    }

    private static void RegisterR2Pack()
    {
        // Get the user classes subkey.
        var classesKey = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);

        // Add an R2 Key.
        var reloadedPackKey = classesKey?.CreateSubKey(Loader.Update.Packs.Constants.Extension);
        reloadedPackKey?.CreateSubKey(@"shell\open\command")?.SetValue("", $"\"{Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe")}\" {Constants.ParameterR2Pack} \"%1\"");
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e) => Errors.HandleException((Exception) e.ExceptionObject);

    /// <summary>
    /// Updates the default config.
    /// </summary>
    private static async Task UpdateDefaultConfig()
    {
        var config = IoC.Get<LoaderConfig>();
        SetLoaderPaths(config, Paths.CurrentProgramFolder);
        await IConfig<LoaderConfig>.ToPathAsync(config, Paths.LoaderConfigPath).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets up services which can be used by the various viewmodels.
    /// </summary>
    private static void SetupServices()
    {
        var config = IoC.Get<LoaderConfig>();
        var synchronizationContext = Actions.SynchronizationContext;

        IoC.BindToConstant(IProcessWatcher.Get());
        IoC.BindToConstant(new ApplicationConfigService(config, synchronizationContext));
        
        var modConfigService = new ModConfigService(config, synchronizationContext);
        IoC.BindToConstant(modConfigService);
        IoC.BindToConstant(new ModUserConfigService(config, modConfigService, synchronizationContext));
    }

    /// <summary>
    /// Sets up viewmodels to be used in the individual mod loader pages.
    /// </summary>
    private static void SetupViewModels()
    {
        var config = IoC.Get<LoaderConfig>();
        IoC.GetConstant<MainPageViewModel>();
        IoC.BindToConstant<IModPackImageConverter>(new JxlImageConverter());

        try
        {
            IoC.BindToConstant(new AggregateNugetRepository(config.NuGetFeeds));
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

        config.UpdatePaths(launcherDirectory, Resources.ErrorLoaderNotFound.Get());
        
        // Update Environment Variables
        Task.Run(() => Environment.SetEnvironmentVariable("RELOADEDIIMODS", config.GetModConfigDirectory(), EnvironmentVariableTarget.User));
    }

    /// <summary>
    /// Checks for mod loader updates.
    /// </summary>
    private static async Task CheckForUpdatesAsync()
    {
        // The action below is destructive.
        // It may remove update metadata for missing dependencies.
        // Don't run this unless we have all the mods.
        if (Update.CheckMissingDependencies().AllAvailable)
            await DependencyMetadataWriterFactory.ExecuteAllAsync(IoC.Get<ModConfigService>());

        await Update.CheckForLoaderUpdatesAsync();
        await Task.Run(Update.CheckForModUpdatesAsync);
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