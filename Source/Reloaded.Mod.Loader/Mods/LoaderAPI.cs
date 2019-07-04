using System;
using System.Reflection;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Mod.Loader.Mods
{
    public class LoaderAPI : IModLoader
    {
        private Loader _loader;

        public LoaderAPI(Loader loader)
        {
            _loader = loader;
        }

        /* Interface Implementation */
        public Version GetLoaderVersion()           => Assembly.GetExecutingAssembly().GetName().Version;
        public IApplicationConfig GetAppConfig()    => _loader.ThisApplication;

        public (IMod, IModConfig)[] GetActiveMods()
        {
            throw new NotImplementedException();
        }

        public ILogger GetLogger()
        {
            throw new NotImplementedException();
        }

        public Action OnModLoaderInitialized            { get; } = () => { };
        public Action<IMod, IModConfig> ModUnloading    { get; } = (a, b) => { };
        public Action<IMod, IModConfig> ModLoading      { get; } = (a, b) => { };
        public Action<IMod, IModConfig> ModUnloaded     { get; } = (a, b) => { };
        public Action<IMod, IModConfig> ModLoaded       { get; } = (a, b) => { };
        public (IMod, T)[] MakeInterfaces<T>()
        {
            throw new NotImplementedException();
        }

        public void AddController<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public void RemoveController<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public T[] GetController<T>()
        {
            throw new NotImplementedException();
        }
    }
}
