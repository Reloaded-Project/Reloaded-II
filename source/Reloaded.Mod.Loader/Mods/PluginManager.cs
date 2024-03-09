using Type = System.Type;

namespace Reloaded.Mod.Loader.Mods;

#nullable enable
/// <summary>
/// A general loader based on <see cref="McMaster.NETCore.Plugins"/> that loads individual mods as plugins.
/// </summary>
public class PluginManager : IDisposable
{
    public LoaderAPI LoaderApi { get; }
    private Logger Logger => _loader.Logger;

    private static readonly Type[] DefaultExportedTypes = new Type[0];
    private static readonly Type[] SharedTypes = { typeof(IModLoader), typeof(IMod) };

    private readonly Dictionary<string, ModInstance> _modifications = new();
    private readonly Dictionary<string, Type[]> _modIdToExports = new();  
    private readonly Dictionary<string, string> _modIdToFolder  = new(); // Maps Mod ID to folder containing mod.

    private LoadContext _sharedContext;
    private readonly Loader _loader;

    /// <summary>
    /// Initializes the <see cref="PluginManager"/>
    /// </summary>
    /// <param name="loader">Instance of the mod loader.</param>
    /// <param name="sharedContext">Used only for testing. Sets shared load context used for plugins.</param>
    public PluginManager(Loader loader, LoadContext? sharedContext = null)
    {
        _loader = loader;
        LoaderApi = new LoaderAPI(_loader);
        _sharedContext = sharedContext ?? LoadContext.BuildSharedLoadContext();
    }

    public void Dispose()
    {
        foreach (var modification in _modifications.Values)
        {
            modification.Dispose();
        }
    }

    /// <summary>
    /// Retrieves a list of all loaded modifications.
    /// </summary>
    public IReadOnlyCollection<ModInstance> GetModifications() => (IReadOnlyCollection<ModInstance>) _modifications.Values;

    /// <summary>
    /// Retrieves the directory of a mod with a specific Mod ID.
    /// </summary>
    /// <param name="modId">The mod id of the mod.</param>
    /// <returns>The directory containing the mod.</returns>
    public string GetDirectoryForModId(string modId)
    {
        return _modIdToFolder[modId];
    }

    /// <summary>
    /// Returns true if a mod is loaded, else false.
    /// </summary>
    public bool IsModLoaded(string modId)
    {
        return _modifications.ContainsKey(modId);
    }

    /// <summary>
    /// Loads a collection of mods.
    /// </summary>
    /// <param name="modPaths">Tuples of individual mod configurations and the paths to those configurations.</param>
    public void LoadMods(List<PathTuple<ModConfig>> modPaths)
    {
        /* Load mods. */
        if (modPaths.Count <= 0) 
            return;

        var watch = Stopwatch.StartNew();
        Logger?.LogWriteLineAsync($"Loading {modPaths.Count} Mod(s).");
        foreach (var mod in modPaths)
        {
            watch.Restart();
            
            if (IsModLoaded(mod.Config.ModId))
                throw new ReloadedException(Errors.ModAlreadyLoaded(mod.Config.ModId));
            
            // Native Mods use separate load logic.
            ModInstance instance;
            if (!mod.Config.HasDllPath())
                instance = PrepareNonDllMod(mod);
            else if (mod.Config.IsNativeMod(mod.Path))
                instance = PrepareNativeMod(mod);
            else
                instance = PrepareDllMod(mod);

            Logger?.LogWriteLineAsync(String.Empty);
            Logger?.LogWriteLineAsync($"Loading: {mod.Config.ModName}");
            Logger?.LogWriteLineAsync($"- AppId   : {mod.Config.ModId}");
            StartMod(instance);
            Logger?.LogWriteLineAsync($"- LoadTime: {watch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Unloads an individual mod.
    /// </summary>
    public void UnloadMod(string modId)
    {
        var mod = _modifications[modId];
        if (mod == null)
            throw new ReloadedException(Errors.ModToUnloadNotFound(modId));

        if (!mod.CanUnload)
            throw new ReloadedException(Errors.ModUnloadNotSupported(modId));

        LoaderApi.ModUnloading(mod.Mod, mod.ModConfig);
        _modIdToFolder.Remove(modId, out _);
        _modIdToExports.Remove(modId, out _);
        _modifications.Remove(modId, out _);
        mod.Dispose();
    }

    /// <summary>
    /// Suspends an individual mod.
    /// </summary>
    public void SuspendMod(string modId)
    {
        var mod = _modifications[modId];
        if (mod == null)
            throw new ReloadedException(Errors.ModToSuspendNotFound(modId));

        if (!mod.CanSuspend)
            throw new ReloadedException(Errors.ModSuspendNotSupported(modId));

        mod.Suspend();
    }

    /// <summary>
    /// Resumes an individual mod.
    /// </summary>
    public void ResumeMod(string modId)
    {
        var mod = _modifications[modId];
        if (mod == null)
            throw new ReloadedException(Errors.ModToResumeNotFound(modId));

        if (!mod.CanSuspend)
            throw new ReloadedException(Errors.ModSuspendNotSupported(modId));

        mod.Resume();
    }

    /// <summary>
    /// Returns a summary of all of the loaded mods in this process.
    /// </summary>
    public ModInstance[] GetLoadedMods()
    {
        return _modifications.Values.ToArray();
    }

    /// <summary>
    /// Returns a summary of all of the loaded mods in this process.
    /// </summary>
    [SkipLocalsInit]
    public ModInfo[] GetLoadedModInfo()
    {
        var modCount = _modifications.Count;
        var allModInfo = GC.AllocateUninitializedArray<ModInfo>(modCount);

        int currentIndex = 0;
        foreach (var entry in _modifications)
            allModInfo[currentIndex++] = new ModInfo(entry.Value.State, entry.Value.ModConfig, entry.Value.CanSuspend, entry.Value.CanUnload);

        return allModInfo;
    }

    /* Mod Loading */
    private bool DoesDllExist(string dllPath, PathTuple<ModConfig> mod)
    {
        bool result = File.Exists(dllPath);
        if (!result)
            Logger.LogWriteLineAsync($"DLL Not Found! {Path.GetFileName(dllPath)}\n" +
                                      $"Mod Name: {mod.Config.ModName}, Mod ID: {mod.Config.ModId}\n" +
                                      $"Please re-download the mod. It is either corrupt or you may have downloaded the source code by accident.",
                _loader.Logger.ColorError);

        return result;
    }
    
    private ModInstance PrepareDllMod(PathTuple<ModConfig> mod)
    {
        // Issue: Share types of mod with mod itself.
        // Load the Mod
        var modId = mod.Config.ModId;
        var dllPath = mod.Config.GetDllPath(mod.Path);
        _modIdToFolder[modId] = Path.GetFullPath(Path.GetDirectoryName(mod.Path)!);
        
        // Return dummy if no DLL.
        if (!DoesDllExist(dllPath, mod))
            return new ModInstance(mod.Config);
        
        var loadContext = LoadModPlugin(mod, dllPath, true, out var entryPointType, out var exportsType);

        // Handle exports
        var config = mod.Config;
        var configNeedsUpdated = !config.CanUnload.HasValue || !config.HasExports.HasValue;
        var exports = DefaultExportedTypes;
        if (exportsType != null)
        {
            var pluginExports = (IExports) Activator.CreateInstance(exportsType)!;
            var typesList = new List<Type>();
            typesList.AddRange(pluginExports.GetTypes());
            typesList.AddRange(pluginExports.GetTypesEx(new ExportsContext()
            {
                ApplicationConfig = LoaderApi.GetAppConfig()
            }));
            
            var assemblies = LoadTypesIntoSharedContext(typesList);
            exports        = GC.AllocateUninitializedArray<Type>(typesList.Count);

            // Find exports in assemblies that were just loaded into the shared context.
            // If we use the ones from the other mods' ALCs, the other ALC will stay loaded
            // because we are still holding a reference to the exports.
            var assemblyToTypes = new Dictionary<Assembly, Type[]>();
            foreach (var asm in assemblies)
            {
                if (!assemblyToTypes.ContainsKey(asm)) 
                    assemblyToTypes[asm] = asm.GetTypes();
            }

            for (int x = 0; x < assemblies.Length; x++)
            {
                var target        = typesList[x];
                var assemblyTypes = assemblyToTypes[assemblies[x]];
                exports[x]        = assemblyTypes.First(y => y.FullName == target.FullName);
            }
            
            config.HasExports = true;
            _modIdToExports[config.ModId] = exports;
            loadContext.Dispose();
            loadContext = LoadModPlugin(mod, dllPath, false, out entryPointType, out _);
        }
        else
        {
            config.HasExports ??= false;
        }
        
        // Load entrypoint.
        var plugin = (IModV1) Activator.CreateInstance(entryPointType!)!;
        
        // Update Unload State
        if (configNeedsUpdated)
            UpdateConfig(mod, plugin);
            
        return new ModInstance(loadContext, plugin, mod.Config);
    }

    private LoadContext LoadModPlugin(PathTuple<ModConfig> mod, string dllPath, bool searchForExports, out Type? entryPointType, out Type? exportsType)
    {
        try
        {
            // If we don't know if we have exports, we need to make it unloadable, such that if we do actually have exports we can unload.
            var config = mod.Config;
            var loadContext = LoadContext.BuildModLoadContext(dllPath,
                config.HasExports.GetValueOrDefault(true) | config.CanUnload.GetValueOrDefault(false),
                GetExportsForModConfig(mod.Config), _sharedContext.Context);
            var defaultAssembly = loadContext.LoadDefaultAssembly();
            var types = defaultAssembly.GetTypes();

            // Find entry point and exports.
            entryPointType = null;
            exportsType = null;
            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (typeof(IModV1).IsAssignableFrom(type))
                {
                    entryPointType = type;
                    if (!searchForExports)
                        return loadContext;
                }

                if (typeof(IExports).IsAssignableFrom(type))
                    exportsType = type;

                // Most mods don't define exports, so no point adding a check here if we found both when searching for both.
            }

            return loadContext;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading Mod with ModId '{mod.Config.ModId}'. " +
                                $"Normally this shouldn't happen, maybe your mod files are borked\n{ex.Message}", ex);
        }
    }

    private ModInstance PrepareNativeMod(PathTuple<ModConfig> tuple)
    {
        var modId = tuple.Config.ModId;
        var dllPath = tuple.Config.GetNativeDllPath(tuple.Path);
        
        if (!DoesDllExist(dllPath, tuple))
            return new ModInstance(tuple.Config);
        
        _modIdToFolder[modId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path)!);
        return new ModInstance(new NativeMod(dllPath), tuple.Config);
    }

    private ModInstance PrepareNonDllMod(PathTuple<ModConfig> tuple)
    {
        // If invalid file path, get directory.
        // If directory, use directory.
        if (Directory.Exists(tuple.Path))
            _modIdToFolder[tuple.Config.ModId] = Path.GetFullPath(tuple.Path);
        else
            _modIdToFolder[tuple.Config.ModId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path)!);

        return new ModInstance(tuple.Config);
    }

    private void StartMod(ModInstance instance)
    {
        try
        {
            LoaderApi.ModLoading(instance.Mod, instance.ModConfig);
            instance.Start(LoaderApi);
            _modifications[instance.ModConfig.ModId] = instance;
            LoaderApi.ModLoaded(instance.Mod, instance.ModConfig);
        }
        catch (Exception)
        {
            Logger.WriteLine($"Error while starting mod: {instance.ModConfig.ModId}", Logger.ColorRed);
            throw;
        }
    }

    /* Setup for mod loading */
    private Type[] GetExportsForModConfig(IModConfig modConfig)
    {
        var exports = SharedTypes.AsEnumerable();

        // Share the mod's types with the mod itself.
        // The type is already preloaded into the default load context, and as such, will be inherited from the default context.
        // i.e. The version loaded into the default context will be used.
        // This is important because we need a single source for the other mods, i.e. ones which take this one as dependency.
        if (_modIdToExports.ContainsKey(modConfig.ModId))
            exports = exports.Concat(_modIdToExports[modConfig.ModId]);

        foreach (var dep in modConfig.ModDependencies)
        {
            if (_modIdToExports.ContainsKey(dep))
                exports = exports.Concat(_modIdToExports[dep]);
        }

        foreach (var optionalDep in modConfig.OptionalDependencies)
        {
            if (_modIdToExports.ContainsKey(optionalDep))
                exports = exports.Concat(_modIdToExports[optionalDep]);
        }

        return exports.ToArray();
    }

    private Assembly[] LoadTypesIntoSharedContext(IReadOnlyList<Type> types)
    {
        var assemblies = new Assembly[types.Count];
        for (var x = 0; x < types.Count; x++)
        {
            var path = new Uri(types[x].Module.Assembly.Location).LocalPath;
            assemblies[x] = _sharedContext.Context.LoadFromAssemblyPath(path);
        }

        return assemblies;
    }
    
    /* Maintenance */
    private void UpdateConfig(PathTuple<ModConfig> mod, IModV1 plugin)
    {
        Logger.WriteLineAsync("Updating Config w/ HasExports & CanUnload.");
        mod.Config.CanUnload = plugin.CanUnload();
        mod.Save();
    }

    /* Internal API */
    internal Type[] GetExportsForModId(string modId) => _modIdToExports[modId];
}
#nullable disable