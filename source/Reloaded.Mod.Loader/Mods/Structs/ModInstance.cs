namespace Reloaded.Mod.Loader.Mods.Structs;

/// <summary>
/// Represents an individual loaded mod managed mod instance.
/// </summary>
public class ModInstance : IDisposable
{
    public LoadContext Context { get; private set; }
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
    public ModInstance(LoadContext context, IModV1 mod, IModConfig config)
    {
        Context = context;
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
            Context?.Dispose();
                
            // Clean up references.
            Context = null;
            Mod = null;

            GC.SuppressFinalize(this);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

            // Blocking GC happens here to ensure no reference to unloaded assembly still exists.
        }
    }

    public void Start(IModLoader loader)
    {
        if (!_started)
        {
            if (Mod != null)
            {
                if (Mod is IModV2 modV2)
                    modV2.StartEx(loader, ModConfig);
                else
                    Mod.Start(loader);
            }
                
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