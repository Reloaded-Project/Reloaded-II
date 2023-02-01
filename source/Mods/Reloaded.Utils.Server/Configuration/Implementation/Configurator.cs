namespace Reloaded.Utils.Server.Configuration.Implementation;

public class Configurator : IConfiguratorV2
{
    /* Migration Guide from V1 to V2:
        - Copy this class to your V1 mod; if you use multiple config files, re-add any changes made to `MakeConfigurations`.
        - Uncomment line in `Migrate` function.
        - In Program.cs (or your Mod Initialisation Code) make new configurator like this. 
        
            // config.ModId has the string ID of your mod.
            var configDirectory = _modLoader.GetModConfigDirectory(config.ModId);
            var modDirectory    = _modLoader.GetDirectoryForModId(config.ModId);
            _configurator 		= new Configurator(configDirectory);
            _configurator.Migrate(modDirectory, _configurator.ConfigFolder);
            
            // Where previously code might have looked like this:
            _configurator = new Configurator(_modLoader.GetDirectoryForModId("your.mod.id"));
    */

    /// <summary>
    /// The folder where the modification files are stored.
    /// </summary>
    public string? ModFolder { get; private set; }

    /// <summary>
    /// Full path to the config folder.
    /// </summary>
    public string? ConfigFolder { get; private set; }

    /// <summary>
    /// Returns a list of configurations.
    /// </summary> 
    public IUpdatableConfigurable[] Configurations => _configurations ?? MakeConfigurations();
    private IUpdatableConfigurable[]? _configurations;

    private IUpdatableConfigurable[] MakeConfigurations()
    {
        _configurations = new IUpdatableConfigurable[]
        {
            // Add more configurations here if needed.
            Configurable<Config>.FromFile(Path.Combine(ConfigFolder!, "Config.json"), "Default Config")
        };

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
    public void Migrate(string oldDirectory, string newDirectory)
    {
        // Uncomment and Modify if needed if migrating from older version.
        // TryMoveFile("Config.json");

#pragma warning disable CS8321
        void TryMoveFile(string fileName)
        {
            try { File.Move(Path.Combine(oldDirectory, fileName), Path.Combine(newDirectory, fileName)); }
            catch (Exception) { /* Ignored */ }
        }
#pragma warning restore CS8321
    }

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
    /// Returns a list of user configurations.
    /// </summary>
    public IConfigurable[] GetConfigurations() => Configurations;

    /// <summary>
    /// Allows for custom launcher/configurator implementation.
    /// If you have your own configuration program/code, run that code here and return true, else return false.
    /// </summary>
    public bool TryRunCustomConfiguration() => false;

    /// <summary>
    /// Sets the mod directory for the Configurator.
    /// </summary>
    public void SetModDirectory(string modDirectory) { ModFolder = modDirectory; }
}