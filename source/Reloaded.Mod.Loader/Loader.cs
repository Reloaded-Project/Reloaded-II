using Console = Reloaded.Mod.Loader.Logging.Console;
using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Loader;

public class Loader : IDisposable
{
    public bool IsLoaded { get; private set; }
    public IApplicationConfig Application { get; private set; }
    public Logger Logger { get; }
    public Console Console { get; }
    public LogWriter LogWriter { get; }
    public PluginManager Manager { get; private set; }
    public LoaderConfig LoaderConfig { get; private set; }

    /// <summary>
    /// This flag suppresses certain exceptions and should only be set to true in unit tests.
    /// </summary>
    public bool IsTesting { get; private set; }

    /// <summary>
    /// Initialize the loader.
    /// </summary>
    public Loader(bool isTesting = false)
    {
        IsTesting = isTesting;
        LoaderConfig = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);
        Logger  = new Logger();
        Console = new Console(LoaderConfig.ShowConsole, Logger, Environment.IsWine ? (IConsoleProxy) new SystemConsoleProxy() : new ColorfulConsoleProxy());

        if (isTesting)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var location    = executingAssembly.Location;
            Manager = new PluginManager(this, new LoadContext(AssemblyLoadContext.GetLoadContext(executingAssembly), location));
        }
        else
        {
            LogWriter = new LogWriter(Logger, Paths.LogPath);
            Manager = new PluginManager(this);
        }
    }

    ~Loader()
    {
        Dispose();
    }

    public void Dispose()
    {
        Manager?.Dispose();
        LogWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    /* Public Interface */

    public void LoadMod(string modId)
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);

        // Check for duplicate.
        if (Manager.IsModLoaded(modId))
            throw new ReloadedException(Errors.ModAlreadyLoaded(modId));

        // Note: Code below already ensures no duplicates but it would be nice to
        // throw for the end users of the loader servers so they can see the error.
        var mod      = FindMod(modId, out var allMods);
        var modArray = new[] {(ModConfig) mod.Config};
        LoadModsWithDependencies(modArray, allMods);
    }

    public void UnloadMod(string modId)
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
        Manager.UnloadMod(modId);
    }

    public void SuspendMod(string modId)
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
        Manager.SuspendMod(modId);
    }

    public void ResumeMod(string modId)
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
        Manager.ResumeMod(modId);
    }

    public ModInfo[] GetLoadedModInfo()
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
        return Manager.GetLoadedModInfo();
    }

    /* Methods */

    public void LoadForCurrentProcess()
    {
        var application = FindThisApplication();
        Wrappers.ThrowIfNull(application, Errors.UnableToFindApplication);
        LoadForAppConfig(application);
    }

    /// <summary>
    /// Loads all mods directly into the process.
    /// </summary>
    public void LoadForAppConfig(IApplicationConfig applicationConfig)
    {
        Wrappers.ThrowIfENotEqual(IsLoaded, false, Errors.ModLoaderAlreadyInitialized);
        Application   = applicationConfig;

        // Get all mods and their paths.
        var allModsForApplication  = ApplicationConfig.GetAllMods(Application, out var allMods, LoaderConfig.GetModConfigDirectory());

        // Get list of mods to load and load them.
        var modsToLoad = allModsForApplication.Where(x => x.Enabled).Select(x => x.Generic.Config);
        LoadModsWithDependencies(modsToLoad, allMods);
        Manager.LoaderApi.OnModLoaderInitialized();
        IsLoaded = true;
    }

    /// <summary>
    /// Gets a list of all mods from filesystem and returns a mod with a matching ModId.
    /// </summary>
    /// <param name="modId">The modId to find.</param>
    /// <param name="allMods">List of all mod configurations, read during the operation.</param>
    /// <exception cref="ReloadedException">A mod to load has not been found.</exception>
    public PathTuple<ModConfig> FindMod(string modId, out List<PathTuple<ModConfig>> allMods)
    {
        // Get mod with ID
        allMods = ModConfig.GetAllMods(LoaderConfig.GetModConfigDirectory());
        var mod = allMods.FirstOrDefault(x => x.Config.ModId == modId);

        if (mod != null)
        {
            var dllPath = mod.Config.GetDllPath(mod.Path);
            return new PathTuple<ModConfig>(dllPath, mod.Config);
        }

        throw new ReloadedException(Errors.ModToLoadNotFound(modId));
    }

    /// <summary>
    /// Loads a collection of mods with their associated dependencies.
    /// </summary>
    internal void LoadModsWithDependencies(IEnumerable<ModConfig> modsToLoad, List<PathTuple<ModConfig>> allMods = null)
    {
        // Cache configuration paths for all mods.
        if (allMods == null)
            allMods = ModConfig.GetAllMods(LoaderConfig.GetModConfigDirectory());

        var configToPathDictionary = new Dictionary<ModConfig, string>();
        foreach (var mod in allMods)
            configToPathDictionary[mod.Config] = mod.Path;

        // Get dependencies, sort and load in order.
        var dependenciesToLoad  = GetDependenciesForMods(modsToLoad, allMods.Select(x => x.Config), LoaderConfig.GetModConfigDirectory());
        var allUniqueModsToLoad = modsToLoad.Concat(dependenciesToLoad).Distinct();
        var allSortedModsToLoad = ModConfig.SortMods(allUniqueModsToLoad);

        var modPaths            = new List<PathTuple<ModConfig>>();
        foreach (var modToLoad in allSortedModsToLoad)
        {
            // Reloaded does not allow loading same mod multiple times.
            if (! Manager.IsModLoaded(modToLoad.ModId))
                modPaths.Add(new PathTuple<ModConfig>(configToPathDictionary[modToLoad], modToLoad));
        }

        Manager.LoadMods(modPaths);
    }

    /// <summary>
    /// Retrieves all of the dependencies for a given set of mods.
    /// </summary>
    /// <exception cref="FileNotFoundException">A dependency for any of the mods has not been found.</exception>
    private HashSet<ModConfig> GetDependenciesForMods(IEnumerable<ModConfig> mods, IEnumerable<ModConfig> allMods, string modDirectory)
    {
        if (allMods == null)
            allMods = ModConfig.GetAllMods(LoaderConfig.GetModConfigDirectory()).Select(x => x.Config);

        var dependencies = ModConfig.GetDependencies(mods, allMods, modDirectory);
        if (dependencies.MissingConfigurations.Count > 0 && !IsTesting)
        {
            string missingMods = String.Join(",", dependencies.MissingConfigurations);
            throw new FileNotFoundException($"Reloaded II was unable to find all dependencies for the mod(s) to be loaded.\n" +
                                            $"Aborting load.\n" +
                                            $"Missing dependencies: {missingMods}");
        }

        return dependencies.Configurations;
    }

    /// <summary>
    /// Searches for the application configuration corresponding to the current 
    /// executing application
    /// </summary>
    private IApplicationConfig FindThisApplication()
    {
        var configurations = ApplicationConfig.GetAllApplications(LoaderConfig.GetApplicationConfigDirectory());
        var fullPath       = NormalizePath(Environment.CurrentProcessLocation.Value);
        Logger.LogWriteLineAsync($"Current Process Location: {fullPath}");

        foreach (var configuration in configurations)
        {
            var application = configuration.Config;
            var appLocation = ApplicationConfig.GetAbsoluteAppLocation(configuration);
                
            if (string.IsNullOrEmpty(appLocation))
                continue;

            var fullAppLocation = NormalizePath(appLocation);
            if (fullAppLocation.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                return application;
        }
        
        // In case of GamePass, binary locations can change after App updates (thanks Microsoft!)
        // So as last resort, we'll match against the AppId.
        Logger.LogWriteLineAsync($"Can't match by App Path, Matching by AppId!", Logger.ColorWarning);
        var expectedAppId = Path.GetFileName(fullPath)!.ToLower();
        foreach (var configuration in configurations)
        {
            if (configuration.Config.AppId.Equals(expectedAppId, StringComparison.OrdinalIgnoreCase))
                return configuration.Config;
        }
        
        return null;
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Loads a mod, with shared types being imported into current AssemblyLoadContext.
    /// </summary>
    /// <param name="modId">The id of the mod to load.</param>
    internal void LoadWithExportsIntoCurrentALC(string modId)
    {
        var mod      = FindMod(modId, out var allMods);
        var modArray = new[] { mod.Config };
        LoadModsWithDependencies(modArray, allMods);
        
        var types = Manager.GetExportsForModId(modId);
        var currentAlc = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        
        // Add event to resolve types if needed.
        currentAlc.Resolving += (context, name) =>
        {
            foreach (var type in types)
            {
                // Check if the type has same name
                if (type.Assembly.GetName().Name != name.Name) 
                    continue;
                
                var alc = AssemblyLoadContext.GetLoadContext(type.Assembly);
                if (alc == context) 
                    continue;
                
                var result = alc.LoadFromAssemblyName(name);
                if (result != null)
                    return result;
            }

            return null;
        };
    }
}