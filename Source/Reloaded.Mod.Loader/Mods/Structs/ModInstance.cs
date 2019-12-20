using System;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Loader.Mods.Structs
{
    /// <summary>
    /// Represents an individual loaded mod managed mod instance.
    /// </summary>
    public class ModInstance : IDisposable
    {
        public PluginLoader Loader { get; private set; }
        public IModV1 Mod { get; private set; }

        public IModConfig ModConfig { get; set; }
        public ModState State { get; set; }
        public bool CanSuspend { get; set; }
        public bool CanUnload { get; set; }

        private bool _started;

        /* Non-Dll Mods */
        public ModInstance(IModConfig config)
        {
            ModConfig  = config;
            CanSuspend = false;
            CanUnload  = true;
        }

        /* Native Mods */
        public ModInstance(IModV1 mod, IModConfig config)
        {
            Mod = mod;
            ModConfig = config;

            CanSuspend = mod.CanSuspend();
            CanUnload = mod.CanUnload();
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
            if (CanSuspend)
            {
                Mod?.Resume();
                State = ModState.Running;
            }
        }

        public void Suspend()
        {
            if (CanSuspend)
            {
                Mod?.Suspend();
                State = ModState.Suspended;
            }
        }
    }
}
