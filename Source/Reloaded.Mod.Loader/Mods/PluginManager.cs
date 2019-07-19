using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using McMaster.NETCore.Plugins;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server.Messages;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Server;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Loader.Mods
{
    /// <summary>
    /// A general loader based on <see cref="McMaster.NETCore.Plugins"/> that loads individual mods as plugins.
    /// </summary>
    public class PluginManager : IDisposable
    {
        public LoaderAPI LoaderApi { get; }
        private readonly Dictionary<string, ModInstance> _modifications = new Dictionary<string, ModInstance>();
        private readonly Loader _loader;

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
        /// Loads a collection of mods from a given set of paths.
        /// </summary>
        /// <param name="modPaths">List of paths to load mods from.</param>
        public void LoadMods(IEnumerable<PathGenericTuple<IModConfig>> modPaths)
        {
            foreach (var modPath in modPaths)
            {
                LoadMod(modPath);
            }
        }

        /// <summary>
        /// Loads an individual DLL/mod.
        /// </summary>
        /// <exception cref="ArgumentException">Mod with specified ID is already loaded.</exception>
        public void LoadMod(PathGenericTuple<IModConfig> tuple)
        {
            // Check if mod with ID already loaded.
            if (_modifications.ContainsKey(tuple.Object.ModId))
                throw new ReloadedException(Errors.ModAlreadyLoaded(tuple.Object.ModId));
            
            // TODO: Native mod loading support.
            var loader = PluginLoader.CreateFromAssemblyFile(tuple.Path, 
                new []{ typeof(IModLoaderV1), typeof(IModV1) },
                config =>
                {
                    config.IsUnloadable = true;
                    config.PreferSharedTypes = true;
                });

            var defaultAssembly = loader.LoadDefaultAssembly();
            var types = defaultAssembly.GetTypes();
            var entryPoint = types.FirstOrDefault(t => typeof(IModV1).IsAssignableFrom(t) && !t.IsAbstract);

            // Load entrypoint.
            var plugin = (IModV1)Activator.CreateInstance(entryPoint);
            var modInstance = new ModInstance(loader, plugin, tuple.Object);
            StartModInstance(modInstance);
            _modifications[tuple.Object.ModId] = modInstance;
        }

        /// <summary>
        /// Unloads an individual mod.
        /// </summary>
        public void UnloadMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null)
            {
                if (mod.Mod.CanUnload())
                {
                    LoaderApi.ModUnloading(mod.Mod, mod.ModConfig);
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
                if (mod.Mod.CanSuspend())
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
                if (mod.Mod.CanSuspend())
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
                allModInfo.Add(new ModInfo(entry.Value.State, entry.Key, entry.Value.CanSuspend, entry.Value.CanSuspend));

            return allModInfo;
        }

        /* Helper methods. */
        private void StartModInstance(ModInstance instance)
        {
            LoaderApi.ModLoading(instance.Mod, instance.ModConfig);
            instance.Start(LoaderApi);
            LoaderApi.ModLoaded(instance.Mod, instance.ModConfig);
        }
    }
}
