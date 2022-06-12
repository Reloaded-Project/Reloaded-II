using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Template.Configuration;
using Reloaded.Mod.Template.Configuration.Implementation;
using System;
#if DEBUG
using System.Diagnostics;
#endif

namespace Reloaded.Mod.Template;

public class Program : IMod
{
    /// <summary>
    /// Used for writing text to the Reloaded log.
    /// </summary>
    private ILogger _logger = null!;

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private IModLoader _modLoader = null!;

    /// <summary>
    /// Stores the contents of your mod's configuration. Automatically updated by template.
    /// </summary>
    private Config _configuration = null!;

    /// <summary>
    /// An interface to Reloaded's the function hooks/detours library.
    /// See: https://github.com/Reloaded-Project/Reloaded.Hooks
    ///      for documentation and samples. 
    /// </summary>
    private IReloadedHooks _hooks = null!;

    /// <summary>
    /// Configuration of the current mod.
    /// </summary>
    private IModConfig _modConfig = null!;

    /// <summary>
    /// Encapsulates your mod logic.
    /// </summary>
    private Mod _mod = null!;

    /// <summary>
    /// Entry point for your mod.
    /// </summary>
    public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
    {
#if DEBUG
        // Attaches debugger in debug mode; ignored in release.
        Debugger.Launch();
#endif

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        _modLoader = (IModLoader) loaderApi;
        _modConfig = (IModConfig) modConfig;
        _logger = (ILogger)_modLoader.GetLogger();
        _modLoader.GetController<IReloadedHooks>().TryGetTarget(out _hooks!);

        // Your config file is in Config.json.
        // Need a different name, format or more configurations? Modify the `Configurator`.
        // If you do not want a config, remove Configuration folder and Config class.
        var configurator = new Configurator(_modLoader.GetModConfigDirectory(_modConfig.ModId));
        _configuration = configurator.GetConfiguration<Config>(0);
        _configuration.ConfigurationUpdated += OnConfigurationUpdated;

        // Please put your mod code in the class below,
        // use this class for only interfacing with mod loader.
        _mod = new Mod(_hooks, _logger);
    }

    private void OnConfigurationUpdated(IConfigurable obj)
    {
        /*
            This is executed when the configuration file gets 
            updated by the user at runtime.
        */

        // Replace configuration with new.
        _configuration = (Config)obj;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");

        // Apply settings from configuration.
        // ... your code here.
    }

    /* Mod loader actions. */
    public void Suspend()
    {
        /*  Some tips if you wish to support this (CanSuspend == true)
         
            A. Undo memory modifications.
            B. Deactivate hooks. (Reloaded.Hooks Supports This!)
        */
    }

    public void Resume()
    {
        /*  Some tips if you wish to support this (CanSuspend == true)
         
            A. Redo memory modifications.
            B. Re-activate hooks. (Reloaded.Hooks Supports This!)
        */
    }

    public void Unload()
    {
        /*  Some tips if you wish to support this (CanUnload == true).
         
            A. Execute Suspend(). [Suspend should be reusable in this method]
            B. Release any unmanaged resources, e.g. Native memory.
        */
    }

    /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
        If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
    */
    public bool CanUnload() => false;
    public bool CanSuspend() => false;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; } = null!;
}