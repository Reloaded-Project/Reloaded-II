#if (IncludeConfig)
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Template.Configuration;

namespace Reloaded.Mod.Template.Template.Configuration;

public class Configurator : IConfiguratorV3
{
    private static ConfiguratorMixin _configuratorMixin = new ConfiguratorMixin();

    /// <summary>
    /// The folder where the modification files are stored.
    /// </summary>
    public string? ModFolder { get; private set; }

    /// <summary>
    /// Full path to the config folder.
    /// </summary>
    public string? ConfigFolder { get; private set; }

    /// <summary>
    /// Specifies additional information for the configurator.
    /// </summary>
    public ConfiguratorContext Context { get; private set; }

    /// <summary>
    /// Returns a list of configurations.
    /// </summary> 
    public IUpdatableConfigurable[] Configurations => _configurations ?? MakeConfigurations();
    private IUpdatableConfigurable[]? _configurations;

    private IUpdatableConfigurable[] MakeConfigurations()
    {
        _configurations = _configuratorMixin.MakeConfigurations(ConfigFolder!);

        // Add self-updating to configurations.
        for (int x = 0; x < Configurations.Length; x++)
        {
            var xCopy = x;
            Configurations[x].ConfigurationUpdated += configurable =>
            {
                Configurations[xCopy] = configurable;
            };
        }

        return _configurations;
    }

    public Configurator() { }
    public Configurator(string configDirectory) : this()
    {
        ConfigFolder = configDirectory;
    }

    /* Configurator V2 */

    /// <summary>
    /// Migrates from the old config location to the newer config location.
    /// </summary>
    /// <param name="oldDirectory">Old directory containing the mod configs.</param>
    /// <param name="newDirectory">New directory pointing to user config folder.</param>
    public void Migrate(string oldDirectory, string newDirectory) => _configuratorMixin.Migrate(oldDirectory, newDirectory);

    /* Configurator */

    /// <summary>
    /// Gets an individual user configuration.
    /// </summary>
    public TType GetConfiguration<TType>(int index) => (TType)Configurations[index];

    /* IConfigurator. */

    /// <summary>
    /// Sets the config directory for the Configurator.
    /// </summary>
    public void SetConfigDirectory(string configDirectory) => ConfigFolder = configDirectory;

    /// <summary>
    /// Specifies additional context for the configurator.
    /// </summary>
    public void SetContext(in ConfiguratorContext context) => Context = context;

    /// <summary>
    /// Returns a list of user configurations.
    /// </summary>
    public IConfigurable[] GetConfigurations() => Configurations;

    /// <summary>
    /// Allows for custom launcher/configurator implementation.
    /// If you have your own configuration program/code, run that code here and return true, else return false.
    /// </summary>
    public bool TryRunCustomConfiguration() => _configuratorMixin.TryRunCustomConfiguration(this);

    /// <summary>
    /// Sets the mod directory for the Configurator.
    /// </summary>
    public void SetModDirectory(string modDirectory) { ModFolder = modDirectory; }
}
#endif