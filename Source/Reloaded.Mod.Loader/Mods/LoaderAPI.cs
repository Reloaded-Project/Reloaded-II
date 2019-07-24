using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Reloaded.Mod.Loader.Mods
{
    // ReSharper disable once InconsistentNaming
    public class LoaderAPI : IModLoader
    {
        /* Controller to mod mapping for GetController<T>, MakeInterfaces<T> */
        private readonly ConcurrentDictionary<Type, ModGenericTuple<object>> _controllerModMapping = new ConcurrentDictionary<Type, ModGenericTuple<object>>();
        private readonly ConcurrentDictionary<Type, ModGenericTuple<object>> _interfaceModMapping = new ConcurrentDictionary<Type, ModGenericTuple<object>>();
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
            foreach (var mapping in _controllerModMapping.ToArray())
            {
                var mod = mapping.Value.Mod;
                if (mod.Equals(modToRemove))
                {
                    _controllerModMapping.Remove(mapping.Key, out _);
                }
            }
        }

        private void AutoDisposePlugin(IModV1 modToRemove, IModConfigV1 modConfigToRemove)
        {
            // Note: Assumes no copying takes place.
            foreach (var mapping in _interfaceModMapping.ToArray())
            {
                var mod = mapping.Value.Mod;
                if (mod.Equals(modToRemove))
                {
                    _interfaceModMapping.Remove(mapping.Key, out _);
                }
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
        public ILoggerV1 GetLogger()                => _loader.Console;

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
                var defaultAssembly = mod.Loader.LoadDefaultAssembly();
                var entryPoints = defaultAssembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
                foreach (var entryPoint in entryPoints)
                {
                    var instance = (T) Activator.CreateInstance(entryPoint);
                    interfaces.Add(new WeakReference<T>(instance));

                    // Store strong reference in mod loader only.
                    _interfaceModMapping[typeof(T)] = new ModGenericTuple<object>(mod.Mod, instance);
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

            return null;
        }

        /* IModLoader V2 */
        public string GetDirectoryForModId(string modId)
        {
            return _loader.Manager.GetDirectoryForModId(modId);
        }
    }
}
