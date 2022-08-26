#if (IncludeConfig)
using Reloaded.Mod.Template.Configuration;
#endif

namespace Reloaded.Mod.Template.Template;

/// <summary>
/// Base class for implementing mod functionality.
/// </summary>
public class ModBase
{
    /// <summary>
    /// Returns true if the suspend functionality is supported, else false.
    /// </summary>
    public virtual bool CanSuspend() => false;

    /// <summary>
    /// Returns true if the unload functionality is supported, else false.
    /// </summary>
    public virtual bool CanUnload() => false;

    /// <summary>
    /// Suspends your mod, i.e. mod stops performing its functionality but is not unloaded.
    /// </summary>
    public virtual void Suspend()
    {
        /*  Some tips if you wish to support this (CanSuspend == true)
         
            A. Undo memory modifications.
            B. Deactivate hooks. (Reloaded.Hooks Supports This!)
        */
    }

    /// <summary>
    /// Unloads your mod, i.e. mod stops performing its functionality but is not unloaded.
    /// </summary>
    /// <remarks>In most cases, calling suspend here is sufficient.</remarks>
    public virtual void Unload()
    {
        /*  Some tips if you wish to support this (CanUnload == true).
         
            A. Execute Suspend(). [Suspend should be reusable in this method]
            B. Release any unmanaged resources, e.g. Native memory.
        */
    }

    /// <summary>
    /// Automatically called by the mod loader when the mod is about to be unloaded.
    /// </summary>
    public virtual void Disposing()
    {

    }

    /// <summary>
    /// Automatically called by the mod loader when the mod is about to be unloaded.
    /// </summary>
    public virtual void Resume()
    {
        /*  Some tips if you wish to support this (CanSuspend == true)
         
            A. Redo memory modifications.
            B. Re-activate hooks. (Reloaded.Hooks Supports This!)
        */
    }

#if (IncludeConfig)
    public virtual void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
    }
#endif
}