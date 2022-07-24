namespace Reloaded.Utils.Server;

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
    /// Configuration of the current mod.
    /// </summary>
    private IModConfig _modConfig = null!;

    /// <summary>
    /// Encapsulates your mod logic.
    /// </summary>
    private LiteNetLibServer? _lnlServer = null!;

    /// <summary>
    /// Entry point for your mod.
    /// </summary>
    public async void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
    {
        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/
        _modLoader = (IModLoader)loaderApi;
        _modConfig = (IModConfig)modConfig;
        _logger = (ILogger)_modLoader.GetLogger();

        // Your config file is in Config.json.
        // Need a different name, format or more configurations? Modify the `Configurator`.
        // If you do not want a config, remove Configuration folder and Config class.
        var configurator = new Configurator(_modLoader.GetModConfigDirectory(_modConfig.ModId));
        _configuration = configurator.GetConfiguration<Config>(0);
        _configuration.ConfigurationUpdated += OnConfigurationUpdated;

        // Start the server on another thread so we don't delay startup with JIT overhead.
        _lnlServer = await Task.Run(() => LiteNetLibServer.Create(_logger, _modLoader, _configuration));
    }

    private void OnConfigurationUpdated(IConfigurable obj)
    {
        // Replace configuration with new.
        _configuration = (Config)obj;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Restarting Server!");
        _lnlServer?.RestartWithConfig(_configuration);
    }

    /* Mod loader actions. */
    public void Suspend() { /* Not Implemented */ }

    public void Resume() { /* Not Implemented */ }

    public void Unload() { _lnlServer?.Dispose(); }

    /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
        If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
    */
    public bool CanUnload() => true;
    public bool CanSuspend() => false;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; } = null!;
}