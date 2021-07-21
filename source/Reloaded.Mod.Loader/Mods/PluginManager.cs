using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Logging;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.Mod.Loader.Utilities;
using static Reloaded.Mod.Loader.Utilities.LogMessageExtensions;

namespace Reloaded.Mod.Loader.Mods
{
    /// <summary>
    /// A general loader based on <see cref="McMaster.NETCore.Plugins"/> that loads individual mods as plugins.
    /// </summary>
    public class PluginManager : IDisposable
    {
        public LoaderAPI LoaderApi { get; }
        private Logger Logger => _loader?.Logger;

        private static readonly Type[] DefaultExportedTypes = new Type[0];
        private static readonly Type[] SharedTypes = { typeof(IModLoader), typeof(IMod) };

        private readonly ConcurrentDictionary<string, ModInstance> _modifications = new ConcurrentDictionary<string, ModInstance>();
        private readonly ConcurrentDictionary<string, ModAssemblyMetadata> _modIdToMetadata = new ConcurrentDictionary<string, ModAssemblyMetadata>();  
        private readonly ConcurrentDictionary<string, string> _modIdToFolder  = new ConcurrentDictionary<string, string>(); // Maps Mod ID to folder containing mod.

        private LoadContext _sharedContext;
        private readonly Loader _loader;


        /// <summary>
        /// Initializes the <see cref="PluginManager"/>
        /// </summary>
        /// <param name="loader">Instance of the mod loader.</param>
        /// <param name="sharedContext">Used only for testing. Sets shared load context used for plugins.</param>
        public PluginManager(Loader loader, LoadContext sharedContext = null)
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
            if (modPaths.Count > 0)
            {
                ExecuteWithStopwatch("Loading Assembly Metadata for Inter Mod Communication, Determining Unload Support etc.", PreloadAssemblyMetadata, modPaths);
                var modInstances = ExecuteWithStopwatch($"Prepare All Mods (Total)", PrepareAllMods, modPaths);
                ExecuteWithStopwatch($"Initialized All Mods (Total)", StartAllInstances, modInstances);
            }
        }

        private ModInstance[] PrepareAllMods(List<PathTuple<ModConfig>> modPaths)
        {
            var modInstances = new ModInstance[modPaths.Count];
            if (_loader.LoaderConfig.LoadModsInParallel)
            {
                var partitioner = Partitioner.Create(0, modPaths.Count);
                Parallel.ForEach(partitioner, (range, loopState) =>
                {
                    for (int x = range.Item1; x < range.Item2; x++)
                        modInstances[x] = ExecuteWithStopwatch($"Prepared Mod: {modPaths[x].Config.ModId}", GetModInstance, modPaths[x]);
                });
            }
            else
            {
                for (int x = 0; x < modInstances.Length; x++)
                    modInstances[x] = ExecuteWithStopwatch($"Prepared Mod: {modPaths[x].Config.ModId}", GetModInstance, modPaths[x]);
            }
            
            return modInstances;
        }

        private void StartAllInstances(ModInstance[] instances)
        {
            foreach (var instance in instances)
                ExecuteWithStopwatch($"Initialized Mod: {instance.ModConfig.ModId}", StartMod, instance);
        }

        /// <summary>
        /// Unloads an individual mod.
        /// </summary>
        public void UnloadMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null)
            {
                if (mod.CanUnload)
                {
                    LoaderApi.ModUnloading(mod.Mod, mod.ModConfig);
                    _modIdToFolder.Remove(modId, out _);
                    _modIdToMetadata.Remove(modId, out _);
                    _modifications.Remove(modId, out _);
                    mod.Dispose();
                }
                else
                {
                    throw new ReloadedException(Errors.ModUnloadNotSupported(modId));
                }
            }
            else
            {
                throw new ReloadedException(Errors.ModToUnloadNotFound(modId));
            }
        }

        /// <summary>
        /// Suspends an individual mod.
        /// </summary>
        public void SuspendMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null)
            {
                if (mod.CanSuspend)
                {
                    mod.Suspend();
                }
                else
                {
                    throw new ReloadedException(Errors.ModSuspendNotSupported(modId));
                }
            }
            else
            {
                throw new ReloadedException(Errors.ModToSuspendNotFound(modId));
            }
        }

        /// <summary>
        /// Resumes an individual mod.
        /// </summary>
        public void ResumeMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null)
            {
                if (mod.CanSuspend)
                {
                    mod.Resume();
                }
                else
                {
                    throw new ReloadedException(Errors.ModSuspendNotSupported(modId));
                }
            }
            else
            {
                throw new ReloadedException(Errors.ModToResumeNotFound(modId));
            }
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
        public List<ModInfo> GetLoadedModSummary()
        {
            var allModInfo = new List<ModInfo>();

            foreach (var entry in _modifications)
                allModInfo.Add(new ModInfo(entry.Value.State, entry.Key, entry.Value.CanSuspend, entry.Value.CanUnload));

            return allModInfo;
        }

        /* Mod Loading */

        /// <summary>
        /// Obtains an instance of an individual ready to load mod.
        /// To start an instance, call <see cref="StartMod"/>
        /// </summary>
        /// <exception cref="ArgumentException">Mod with specified ID is already loaded.</exception>
        /// <param name="tuple">A tuple of mod config and path to config.</param>
        private ModInstance GetModInstance(PathTuple<ModConfig> tuple)
        {
            // Check if mod with ID already loaded.
            if (IsModLoaded(tuple.Config.ModId))
                throw new ReloadedException(Errors.ModAlreadyLoaded(tuple.Config.ModId));

            // Load DLL or non-dll mod.
            if (tuple.Config.HasDllPath())
            {
                var dllPath = tuple.Config.GetDllPath(tuple.Path);
                if (File.Exists(dllPath))
                    return tuple.Config.IsNativeMod(tuple.Path) ? PrepareNativeMod(tuple) : PrepareDllMod(tuple);
                else
                    Logger?.LogWriteLineAsync($"DLL Not Found! {Path.GetFileName(dllPath)}\n" +
                                   $"Mod Name: {tuple.Config.ModName}, Mod ID: {tuple.Config.ModId}\n" +
                                   $"Please re-download the mod. It is either corrupt or you may have downloaded the source code by accident.",
                                    _loader.Logger.ColorError);
            }

            return PrepareNonDllMod(tuple);
        }

        private ModInstance PrepareDllMod(PathTuple<ModConfig> tuple)
        {
            var modId = tuple.Config.ModId;
            var dllPath = tuple.Config.GetDllPath(tuple.Path);
            _modIdToFolder[modId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path));

            var loadContext     = LoadContext.BuildModLoadContext(dllPath, _modIdToMetadata[modId].IsUnloadable, GetExportsForModConfig(tuple.Config), _sharedContext.Context);
            var defaultAssembly = loadContext.LoadDefaultAssembly();
            var types           = defaultAssembly.GetTypes();
            var entryPoint      = types.FirstOrDefault(t => typeof(IModV1).IsAssignableFrom(t) && !t.IsAbstract);

            // Load entrypoint.
            var plugin = (IModV1) Activator.CreateInstance(entryPoint);
            return new ModInstance(loadContext, plugin, tuple.Config);
        }

        private ModInstance PrepareNativeMod(PathTuple<ModConfig> tuple)
        {
            var modId = tuple.Config.ModId;
            var dllPath = tuple.Config.GetNativeDllPath(tuple.Path);
            _modIdToFolder[modId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path));
            return new ModInstance(new NativeMod(dllPath), tuple.Config);
        }

        private ModInstance PrepareNonDllMod(PathTuple<ModConfig> tuple)
        {
            // If invalid file path, get directory.
            // If directory, use directory.
            if (Directory.Exists(tuple.Path))
                _modIdToFolder[tuple.Config.ModId] = Path.GetFullPath(tuple.Path);
            else
                _modIdToFolder[tuple.Config.ModId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path));

            return new ModInstance(tuple.Config);
        }

        private void StartMod(ModInstance instance)
        {
            LoaderApi.ModLoading(instance.Mod, instance.ModConfig);
            instance.Start(LoaderApi);
            _modifications[instance.ModConfig.ModId] = instance;
            LoaderApi.ModLoaded(instance.Mod, instance.ModConfig);
        }

        /* Setup for mod loading */
        private Type[] GetExportsForModConfig(IModConfig modConfig)
        {
            var exports = SharedTypes.AsEnumerable();

            // Share the mod's types with the mod itself.
            // The type is already preloaded into the default load context, and as such, will be inherited from the default context.
            // i.e. The version loaded into the default context will be used.
            // This is important because we need a single source for the other mods, i.e. ones which take this one as dependency.
            if (_modIdToMetadata.ContainsKey(modConfig.ModId))
                exports = exports.Concat(_modIdToMetadata[modConfig.ModId].Exports);

            foreach (var dep in modConfig.ModDependencies)
            {
                if (_modIdToMetadata.ContainsKey(dep))
                    exports = exports.Concat(_modIdToMetadata[dep].Exports);
            }

            foreach (var optionalDep in modConfig.OptionalDependencies)
            {
                if (_modIdToMetadata.ContainsKey(optionalDep))
                    exports = exports.Concat(_modIdToMetadata[optionalDep].Exports);
            }

            return exports.ToArray();
        }

        private void PreloadAssemblyMetadata(List<PathTuple<ModConfig>> configPathTuples)
        {
            if (_loader.LoaderConfig.LoadModsInParallel)
            {
                var partitioner = Partitioner.Create(0, configPathTuples.Count);
                Parallel.ForEach(partitioner, (tuple, state) =>
                {
                    for (int x = tuple.Item1; x < tuple.Item2; x++)
                        PreloadAssemblyMetadataItem(configPathTuples[x]);
                });
            }
            else
            {
                for (int x = 0; x < configPathTuples.Count; x++)
                    PreloadAssemblyMetadataItem(configPathTuples[x]);
            }
        }

        private void PreloadAssemblyMetadataItem(PathTuple<ModConfig> tuple)
        {
            var dllPath = tuple.Config.GetDllPath(tuple.Path);
            if (!File.Exists(dllPath) || tuple.Config.IsNativeMod(tuple.Path))
                return;

            if (GetMetadataForDllMod(dllPath, out var exports, out bool isUnloadable))
                _modIdToMetadata[tuple.Config.ModId] = new ModAssemblyMetadata(exports, isUnloadable);
        }

        private bool GetMetadataForDllMod(string dllPath, out Type[] exports, out bool isUnloadable)
        {
            exports      = DefaultExportedTypes; // Preventing heap allocation here.
            isUnloadable = false;

            var loadContext     = LoadContext.BuildModLoadContext(dllPath, true, SharedTypes, _sharedContext.Context);
            var defaultAssembly = loadContext.LoadDefaultAssembly();
            var types           = defaultAssembly.GetTypes();

            var exportsEntryPoint = types.FirstOrDefault(t => typeof(IExports).IsAssignableFrom(t) && !t.IsAbstract);
            if (exportsEntryPoint != null)
            {
                var pluginExports = (IExports) Activator.CreateInstance(exportsEntryPoint);
                var exportedTypes = pluginExports.GetTypes();
                var assemblies    = LoadTypesIntoSharedContext(exportedTypes);
                exports           = new Type[exportedTypes.Length];

                // Find exports in assemblies that were just loaded into the default ALC.
                // If we don't do this; the assemblies will stay loaded in the other ALC because we are still holding a reference to them.
                var assemblyToTypes = new Dictionary<Assembly, Type[]>();
                foreach (var asm in assemblies)
                {
                    if (!assemblyToTypes.ContainsKey(asm)) 
                        assemblyToTypes[asm] = asm.GetTypes();
                }

                for (int x = 0; x < assemblies.Length; x++)
                {
                    var target        = exportedTypes[x];
                    var assemblyTypes = assemblyToTypes[assemblies[x]];
                    exports[x]        = assemblyTypes.First(y => y.FullName == target.FullName);
                }
            }

            var modEntryPoint = types.FirstOrDefault(t => typeof(IModV1).IsAssignableFrom(t) && !t.IsAbstract);
            if (modEntryPoint != null)
            {
                var plugin = (IModV1) Activator.CreateInstance(modEntryPoint);
                isUnloadable = plugin.CanUnload();
            }

            loadContext.Dispose();
            return true;
        }

        private Assembly[] LoadTypesIntoSharedContext(IReadOnlyList<Type> types)
        {
            var assemblies = new Assembly[types.Count];
            for (var x = 0; x < types.Count; x++)
            {
                var path = new Uri(types[x].Module.Assembly.CodeBase).LocalPath;
                assemblies[x] = _sharedContext.Context.LoadFromAssemblyPath(path);
            }

            return assemblies;
        }

        /* Utility */

        private void ExecuteWithStopwatch<T>(string message, Action<T> code, T parameter)
        {
            var _stopwatch = new Stopwatch();
            _stopwatch.Start();
            code(parameter);
            Logger?.LogWriteLineAsync($"{message}: Complete {_stopwatch.ElapsedMilliseconds}ms");
        }

        private Y ExecuteWithStopwatch<T, Y>(string message, Func<T, Y> code, T parameter)
        {
            var _stopwatch = new Stopwatch();
            _stopwatch.Start();
            var result = code(parameter);
            Logger?.LogWriteLineAsync($"{message}: Complete {_stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}
