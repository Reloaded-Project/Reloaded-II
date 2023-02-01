namespace Reloaded.Mod.Loader.Mods;

// ReSharper disable once InconsistentNaming
public class LoaderAPI : IModLoader
{
    /* Controller to mod mapping for GetController<T>, MakeInterfaces<T> */
    private readonly ConcurrentDictionary<Type, ModGenericTuple<object>> _controllerModMapping = new ConcurrentDictionary<Type, ModGenericTuple<object>>();
    private readonly ConcurrentDictionary<Type, List<ModGenericTuple<object>>> _interfaceModMapping = new ConcurrentDictionary<Type, List<ModGenericTuple<object>>>();
    private readonly Loader _loader;

    public LoaderAPI(Loader loader)
    {
        _loader = loader;
        ModUnloading += AutoDisposeController;
        ModUnloading += AutoDisposePlugin;
    }

    private void AutoDisposeController(IModV1 modToRemove, IModConfigV1 modConfigToRemove)
    {
        // Note: Assumes no copying takes place.
        AutoDisposeController_NoInline(modToRemove);
        GC.Collect();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AutoDisposeController_NoInline(IModV1 modToRemove)
    {
        foreach (var mapping in _controllerModMapping.ToArray())
        {
            var mod = mapping.Value.Mod;
            if (mod.Equals(modToRemove))
                _controllerModMapping.Remove(mapping.Key, out _);
        }
    }

    private void AutoDisposePlugin(IModV1 modToRemove, IModConfigV1 modConfigToRemove)
    {
        // Note: Assumes no copying takes place.
        AutoDisposePlugin_NoInline(modToRemove);
        GC.Collect();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AutoDisposePlugin_NoInline(IModV1 modToRemove)
    {
        foreach (var modMapping in _interfaceModMapping.ToArray())
        foreach (var mapping in modMapping.Value.ToArray())
        {
            if (mapping.Mod.Equals(modToRemove))
                modMapping.Value.Remove(mapping);
        }
    }

    /* IModLoader V1 */

    /* Events */
    public Action OnModLoaderInitialized                { get; set; } = () => { };
    public Action<IModV1, IModConfigV1> ModLoading      { get; set; } = (a, b) => { };
    public Action<IModV1, IModConfigV1> ModLoaded       { get; set; } = (a, b) => { };
    public Action<IModV1, IModConfigV1> ModUnloading    { get; set; } = (a, b) => { };

    /* Properties */
    public Version GetLoaderVersion()           => Assembly.GetExecutingAssembly().GetName().Version;
    public IApplicationConfigV1 GetAppConfig()  => _loader.Application;
    public ILoggerV1 GetLogger()                => _loader.Logger;

    /* Functions */
    public ModGenericTuple<IModConfigV1>[] GetActiveMods()
    {
        var modifications   = _loader.Manager.GetModifications();
        var activeMods      = new ModGenericTuple<IModConfigV1>[modifications.Count];
        using (var enumerator = modifications.GetEnumerator())
        {
            for (int x = 0; x < modifications.Count; x++)
            {
                enumerator.MoveNext();
                var current = enumerator.Current;
                activeMods[x] = new ModGenericTuple<IModConfigV1>(current.Mod, current.ModConfig);
            }
        }

        return activeMods;
    }

    public WeakReference<T>[] MakeInterfaces<T>() where T : class
    {
        var modifications = _loader.Manager.GetModifications();
        var interfaces = new List<WeakReference<T>>();

        foreach (var mod in modifications)
        {
            var defaultAssembly = mod.Context?.LoadDefaultAssembly();
            var entryPoints = defaultAssembly?.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
            if (entryPoints == null) 
                continue;

            foreach (var entryPoint in entryPoints)
            {
                var instance = (T)Activator.CreateInstance(entryPoint);
                interfaces.Add(new WeakReference<T>(instance));

                // Store strong reference in mod loader only.
                if (!_interfaceModMapping.TryGetValue(typeof(T), out var list))
                {
                    list = new List<ModGenericTuple<object>>();
                    _interfaceModMapping[typeof(T)] = list;
                }

                list.Add(new ModGenericTuple<object>(mod.Mod, instance));
            }
        }

        return interfaces.ToArray();
    }

    public void AddOrReplaceController<T>(IModV1 owner, T instance)
    {
        _controllerModMapping[typeof(T)] = new ModGenericTuple<object>(owner, instance);
    }

    public void RemoveController<T>()
    {
        _controllerModMapping.TryRemove(typeof(T), out _);
    }

    public WeakReference<T> GetController<T>() where T : class
    {
        var tType = typeof(T);
        foreach (var key in _controllerModMapping.Keys)
        {
            if (key.IsAssignableFrom(tType))
            {
                var mapping = _controllerModMapping[key];
                T tGeneric = (T) mapping.Generic;
                return new WeakReference<T>(tGeneric);
            }
        }

        _loader.Logger.LogWriteLine($"[LoaderAPI] Warning: GetController<{typeof(T).Name}> returned null.", _loader.Logger.ColorWarning);
        return null;
    }

    /* IModLoader V2 */
    public string GetDirectoryForModId(string modId)
    {
        return _loader.Manager.GetDirectoryForModId(modId);
    }

    /* IModLoader V3 */
    public string GetModConfigDirectory(string modId)
    {
        var directory = ModUserConfig.GetUserConfigFolderForMod(modId, _loader.LoaderConfig.GetModUserConfigDirectory());
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return directory;
    }

    /* IModLoader V4 */
    public void LoadMod(string modId) => _loader.LoadMod(modId);

    public void UnloadMod(string modId) => _loader.UnloadMod(modId);

    public void SuspendMod(string modId) => _loader.SuspendMod(modId);

    public void ResumeMod(string modId) => _loader.ResumeMod(modId);

    public ModInfo[] GetLoadedMods()
    {
        return _loader.GetLoadedModInfo();
    }
}