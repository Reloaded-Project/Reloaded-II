namespace Reloaded.Mod.Interfaces;

/// <summary>
/// Version 3 of the configurator.  
/// With support for setting context via a method.  
/// </summary>
public interface IConfiguratorV3 : IConfiguratorV2
{
    /// <summary>
    /// Sets the directory where the config is located.
    /// </summary>
    /// <param name="context">Provides additional details for the configurator.</param>
    void SetContext(in ConfiguratorContext context);
}

/// <summary>
/// Provides additional context to the configurator.
/// </summary>
public struct ConfiguratorContext
{
    /// <summary>
    /// The application for which the config is to be edited.
    /// </summary>
    public IApplicationConfigV1 Application;

    /// <summary>
    /// Path to the application config (AppConfig.json).
    /// </summary>
    public string ApplicationConfigPath;

    /// <summary>
    /// Path to the mod config (ModConfig.json).
    /// </summary>
    public string ModConfigPath;
} 