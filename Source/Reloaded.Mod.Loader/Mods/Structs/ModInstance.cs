using System;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Loader.Mods.Structs
{
    /// <summary>
    /// Represents an individual loaded mod instance.
    /// </summary>
    public class ModInstance : IDisposable
    {
        public PluginLoader Loader;
        public IModV1 Mod;
        public IModConfig ModConfig;

        public ModState State;
        public bool CanSuspend;
        public bool CanUnload;
        private bool _started;

        /* Non-Dll Mods */
        public ModInstance(IModConfig config)
        {
            ModConfig = config;
            CanSuspend = false;
            CanUnload = true;
        }

        /* Dll Mods */
        public ModInstance(PluginLoader loader, IModV1 mod, IModConfig config)
        {
            Loader = loader;
            Mod = mod;
            ModConfig = config;

            CanSuspend = mod.CanSuspend();
            CanUnload = mod.CanUnload();
        }

        ~ModInstance()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (CanUnload)
            {
                Mod?.Disposing?.Invoke();
                Mod?.Unload();
                Loader?.Dispose();

                // Clean up references.
                Loader = null;
                Mod = null;
                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.SuppressFinalize(this);

                // Blocking GC happens here to ensure no reference to unloaded assembly still exists.
            }
        }

        public void Start(IModLoader loader)
        {
            if (!_started)
            {
                Mod?.Start(loader);
                State = ModState.Running;
                _started = true;
            }
        }

        public void Resume()
        {
            Mod?.Resume();
            State = ModState.Running;
        }

        public void Suspend()
        {
            Mod?.Suspend();
            State = ModState.Suspended;
        }
    }
}
