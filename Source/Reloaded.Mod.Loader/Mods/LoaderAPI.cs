using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Reloaded.Mod.Loader.Mods
{
    public class LoaderAPI : IModLoader
    {
        /* Controller to mod mapping for GetController<T> */
        private ConcurrentDictionary<Type, ModGenericTuple<object>> _controllerModMapping = new ConcurrentDictionary<Type, ModGenericTuple<object>>();
        private Loader _loader;

        public LoaderAPI(Loader loader)
        {
            _loader = loader;
        }

        /* Interface Implementation */

        /* Events */
        public Action OnModLoaderInitialized                { get; } = () => { };
        public Action<IModV1, IModConfigV1> ModUnloading    { get; } = (a, b) => { };
        public Action<IModV1, IModConfigV1> ModLoading      { get; } = (a, b) => { };
        public Action<IModV1, IModConfigV1> ModLoaded       { get; } = (a, b) => { };

        /* Properties */
        public Version GetLoaderVersion()           => Assembly.GetExecutingAssembly().GetName().Version;
        public IApplicationConfigV1 GetAppConfig()  => _loader.ThisApplication;
        public ILoggerV1 GetLogger()                => _loader.Console;

        /* Functions */
        public ModGenericTuple<IModConfigV1>[] GetActiveMods()
        {
            var modifications   = _loader.Manager.GetModifications();
            var activeMods      = new ModGenericTuple<IModConfigV1>[modifications.Count];
            using (var enumerator = modifications.GetEnumerator())
            {
                enumerator.MoveNext();
                for (int x = 0; x < modifications.Count; x++)
                {
                    var current = enumerator.Current;
                    activeMods[x] = new ModGenericTuple<IModConfigV1>(current.Mod, current.ModConfig);
                }
            }

            return activeMods;
        }

        public ModGenericTuple<T>[] MakeInterfaces<T>()
        {
            var modifications = _loader.Manager.GetModifications();
            var interfaces = new List<ModGenericTuple<T>>();

            foreach (var mod in modifications)
            {
                var defaultAssembly = mod.Loader.LoadDefaultAssembly();
                var entryPoints = defaultAssembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
                foreach (var entryPoint in entryPoints)
                {
                    var instance = (T) Activator.CreateInstance(entryPoint);
                    interfaces.Add(new ModGenericTuple<T>(mod.Mod, instance));
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
            _controllerModMapping.TryRemove(typeof(T), out var old);
        }

        public ModGenericTuple<T> GetController<T>()
        {
            var tType = typeof(T);
            foreach (var key in _controllerModMapping.Keys)
            {
                if (key.IsAssignableFrom(tType))
                {
                    var mapping = _controllerModMapping[key];
                    return new ModGenericTuple<T>(mapping.Mod, (T) mapping.Generic);
                }
            }

            return null;
        }
    }
}
