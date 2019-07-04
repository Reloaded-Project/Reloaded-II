using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using McMaster.NETCore.Plugins;
using Reloaded.Messaging.Messages;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Interfaces;
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
    public class PluginManager
    {
        private Dictionary<string, ModInstance> _modifications = new Dictionary<string, ModInstance>();
        private Loader _loader;
        private LoaderAPI _loaderApi;

        /// <summary>
        /// Initializes the <see cref="PluginManager"/>
        /// </summary>
        /// <param name="modPaths">List of all DLLs to load during initialization</param>
        /// <param name="loader"></param>
        public PluginManager(IEnumerable<PathGenericTuple<IModConfig>> modPaths, Loader loader)
        {
            _loader = loader;
            _loaderApi = new LoaderAPI(_loader);
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
                throw new ArgumentException("Mod with specified ID is already loaded.");
            
            // TODO: Native mod loading support.
            var loaderOptions = PluginLoaderOptions.PreferSharedTypes | PluginLoaderOptions.IsUnloadable;
            var loader = PluginLoader.CreateFromAssemblyFile(tuple.Path, 
                new []{ typeof(IModLoader), typeof(IMod) },
                loaderOptions);

            var defaultAssembly = loader.LoadDefaultAssembly();
            var types = defaultAssembly.GetTypes();
            var entryPoints = types.Where(t => typeof(IMod).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var entryPoint in entryPoints)
            {
                var plugin = (IMod) Activator.CreateInstance(entryPoint);
                var modInstance = new ModInstance(loader, plugin, tuple.Object);
                StartModInstance(modInstance);
                _modifications[tuple.Object.ModId] = modInstance;
            }
        }

        /// <summary>
        /// Unloads an individual mod.
        /// </summary>
        public void UnloadMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null && mod.Mod.CanUnload())
            {
                _modifications.Remove(modId);
                mod.Dispose();
            }
            else
            {
                throw new ReloadedException("Mod to be unloaded not found.");
            }
        }

        /// <summary>
        /// Suspends an individual mod.
        /// </summary>
        public void SuspendMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null && mod.Mod.CanSuspend())
            {
                mod.Suspend();
            }
            else
            {
                throw new ReloadedException("Mod to be suspended not found.");
            }
        }


        /// <summary>
        /// Resumes an individual mod.
        /// </summary>
        public void ResumeMod(string modId)
        {
            var mod = _modifications[modId];
            if (mod != null && mod.Mod.CanSuspend())
            {
                mod.Resume();
            }
            else
            {
                throw new ReloadedException("Mod to be resumed not found.");
            }
        }

        /// <summary>
        /// Retrieves a list of all loaded mods.
        /// </summary>
        public void GetLoadedMods(ref NetMessage<GetLoadedMods> message)
        {
            var allModInfo = new List<ModInfo>();
            foreach (var entry in _modifications)
            {
                allModInfo.Add(new ModInfo(entry.Value.State, entry.Key, entry.Value.CanSuspend, entry.Value.CanSuspend));
            }

            var messageToSend = new Message<MessageType, GetLoadedModsResponse>(new GetLoadedModsResponse(allModInfo.ToArray()));
            message.Peer.Send(messageToSend.Serialize(), DeliveryMethod.ReliableOrdered);
        }

        /* Helper methods. */
        private void StartModInstance(ModInstance instance)
        {
            _loaderApi.ModLoading(instance.Mod, instance.ModConfig);
            instance.Start(_loaderApi);
            _loaderApi.ModLoaded(instance.Mod, instance.ModConfig);
        }
    }
}
