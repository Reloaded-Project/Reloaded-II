using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Loader.Mods
{
    /// <summary>
    /// A general loader based on <see cref="McMaster.NETCore.Plugins"/> that loads individual mods as plugins.
    /// </summary>
    public class PluginManager : IDisposable
    {
        public LoaderAPI LoaderApi { get; }
        private static readonly Type[] SharedTypes = { typeof(IModLoader), typeof(IMod) };

        private readonly Dictionary<string, Type[]>      _modIdToExports = new Dictionary<string, Type[]>();  
        private readonly Dictionary<string, ModInstance> _modifications = new Dictionary<string, ModInstance>();
        private readonly Dictionary<string, string>      _modIdToFolder = new Dictionary<string, string>(); // Maps Mod ID to folder containing mod.
        private readonly Loader _loader;
        private readonly Stopwatch _stopWatch = new Stopwatch();

        /// <summary>
        /// Initializes the <see cref="PluginManager"/>
        /// </summary>
        /// <param name="loader">Instance of the mod loader.</param>
        public PluginManager(Loader loader)
        {
            _loader = loader;
            LoaderApi = new LoaderAPI(_loader);
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
        public IReadOnlyCollection<ModInstance> GetModifications() => _modifications.Values;

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
        /// Loads a collection of mods from a given set of paths.
        /// </summary>
        /// <param name="modPaths">List of paths to load mods from.</param>
        public void LoadMods(IEnumerable<PathGenericTuple<IModConfig>> modPaths)
        {
            PreloadExports(modPaths);

            /* Load mods. */
            foreach (var modPath in modPaths)
            {
                ExecuteWithStopwatch($"Loading Mod: {modPath.Object.ModId}", LoadMod, modPath);
            }
        }

        /// <summary>
        /// Loads an individual DLL/mod.
        /// </summary>
        /// <exception cref="ArgumentException">Mod with specified ID is already loaded.</exception>
        public void LoadMod(PathGenericTuple<IModConfig> tuple)
        {
            // Check if mod with ID already loaded.
            if (IsModLoaded(tuple.Object.ModId))
                throw new ReloadedException(Errors.ModAlreadyLoaded(tuple.Object.ModId));

            // Load DLL or non-dll mod.
            if (File.Exists(tuple.Path))
            {
                LoadDllMod(tuple);
            }
            else
            {
                LoadNonDllMod(tuple);
            }
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
                    _modIdToFolder.Remove(modId);
                    _modIdToExports.Remove(modId);
                    _modifications.Remove(modId);
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
        private void LoadDllMod(PathGenericTuple<IModConfig> tuple)
        {
            _modIdToFolder[tuple.Object.ModId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path));

            // TODO: Native mod loading support.
            var loader = PluginLoader.CreateFromAssemblyFile(tuple.Path,
                GetExportsForModConfig(tuple.Object),
                config =>
                {
                    config.IsUnloadable = true;
                    config.PreferSharedTypes = true;
                });

            var defaultAssembly = loader.LoadDefaultAssembly();
            var types           = defaultAssembly.GetTypes();
            var entryPoint      = types.FirstOrDefault(t => typeof(IModV1).IsAssignableFrom(t) && !t.IsAbstract);

            // Load entrypoint.
            var plugin = (IModV1) Activator.CreateInstance(entryPoint);
            var modInstance = new ModInstance(loader, plugin, tuple.Object);

            StartModInstance(modInstance);
        }

        private void LoadNonDllMod(PathGenericTuple<IModConfig> tuple)
        {
            // If invalid file path, get directory.
            // If directory, use directory.
            if (Directory.Exists(tuple.Path))
                _modIdToFolder[tuple.Object.ModId] = Path.GetFullPath(tuple.Path);
            else
                _modIdToFolder[tuple.Object.ModId] = Path.GetFullPath(Path.GetDirectoryName(tuple.Path));

            var modInstance = new ModInstance(tuple.Object);
            StartModInstance(modInstance);
        }

        private void StartModInstance(ModInstance instance)
        {
            _modifications[instance.ModConfig.ModId] = instance;

            LoaderApi.ModLoading(instance.Mod, instance.ModConfig);
            instance.Start(LoaderApi);
            LoaderApi.ModLoaded(instance.Mod, instance.ModConfig);
        }

        /* Setup for mod loading */
        private Type[] GetExportsForModConfig(IModConfig modConfig)
        {
            var exports = SharedTypes.AsEnumerable();
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

        private void PreloadExports(IEnumerable<PathGenericTuple<IModConfig>> modPaths)
        {
            ExecuteWithStopwatch("Loading Exported Types for Inter Mod Communication.", (paths) =>
            {
                foreach (var modPath in paths)
                {
                    if (!File.Exists(modPath.Path))
                        continue;

                    if (!PreloadExportsForDllMod(modPath.Path, out var exports))
                        continue;

                    _modIdToExports[modPath.Object.ModId] = exports;
                }
            }, modPaths);
        }

        private bool PreloadExportsForDllMod(string dllPath, out Type[] exports)
        {
            var loader = PluginLoader.CreateFromAssemblyFile(dllPath, true, SharedTypes.ToArray());

            var defaultAssembly = loader.LoadDefaultAssembly();
            var types = defaultAssembly.GetTypes();
            var entryPoint = types.FirstOrDefault(t => typeof(IExports).IsAssignableFrom(t) && !t.IsAbstract);

            if (entryPoint != null)
            {
                var plugin = (IExports)Activator.CreateInstance(entryPoint);
                exports = plugin.GetTypes();
                loader.Dispose();

                LoadTypesIntoDefaultContext(exports);
                return true;
            }

            exports = null;
            loader.Dispose();
            return false;
        }

        private void LoadTypesIntoDefaultContext(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var path = new Uri(type.Module.Assembly.CodeBase).LocalPath;
                AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            }
        }

        /* Utility */
        private void WriteLine(string message, string prefix = "[Reloaded] ")
        {
            _loader.Console.WriteLine($"{prefix}{message}");
        }

        private void ExecuteWithStopwatch<T>(string message, Action<T> code, T parameter)
        {
            WriteLine($"{message}");
            _stopWatch.Restart();
            code(parameter);
            _stopWatch.Stop();
            WriteLine($"... {_stopWatch.ElapsedMilliseconds}ms");
        }
    }
}
