namespace Reloaded.Utils.Server.Configuration.Implementation;

public class Configurable<TParentType> : IUpdatableConfigurable where TParentType : Configurable<TParentType>, new()
{
    // Default Serialization Options
    public static JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    /* Events */

    /// <summary>
    /// Automatically executed when the external configuration file is updated.
    /// Passes a new instance of the configuration as parameter.
    /// Inside your event handler, replace the variable storing the configuration with the new one.
    /// </summary>
    [Browsable(false)]
    public event Action<IUpdatableConfigurable>? ConfigurationUpdated;

    /* Class Properties */

    /// <summary>
    /// Full path to the configuration file.
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    public string? FilePath { get; private set; }

    /// <summary>
    /// The name of the configuration file.
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    public string? ConfigName { get; private set; }

    /// <summary>
    /// Receives events on whenever the file is actively changed or updated.
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    private FileSystemWatcher? ConfigWatcher { get; set; }

    /* Construction */
    public Configurable() { }

    private void Initialize(string filePath, string configName)
    {
        // Initializes an instance after construction by e.g. a serializer.
        FilePath = filePath;
        ConfigName = configName;

        MakeConfigWatcher();
        Save = OnSave;
    }

    /* Cleanup */
    public void DisposeEvents()
    {
        // Halts the FilesystemWatcher and all events associated with this instance.
        ConfigWatcher?.Dispose();
        ConfigurationUpdated = null;
    }

    /* Load/Save support. */

    /// <summary>
    /// Saves the configuration to the hard disk.
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    public Action? Save { get; private set; }

    /// <summary>
    /// Safety lock for when changed event gets raised twice on file save.
    /// </summary>
    [Browsable(false)]
    private static object _readLock = new object();

    /// <summary>
    /// Loads a specified configuration from the hard disk, or creates a default if it does not exist.
    /// </summary>
    /// <param name="filePath">The full file path of the config.</param>
    /// <param name="configName">The name of the configuration.</param>
    public static TParentType FromFile(string filePath, string configName) => ReadFrom(filePath, configName);

    /* Event */

    /// <summary>
    /// Creates a <see cref="FileSystemWatcher"/> that will automatically raise an
    /// <see cref="OnConfigurationUpdated"/> event when the config file is changed.
    /// </summary>
    /// <returns></returns>
    private void MakeConfigWatcher()
    {
        ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(FilePath)!, Path.GetFileName(FilePath)!);
        ConfigWatcher.Changed += (sender, e) => OnConfigurationUpdated();
        ConfigWatcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Reloads the configuration from the hard disk and raises the updated event.
    /// </summary>
    private void OnConfigurationUpdated()
    {
        lock (_readLock)
        {
            // Load and copy events.
            // Note: External program might still be writing to file while this is being executed, so we need to keep retrying.
            var newConfig = Utilities.TryGetValue(() => ReadFrom(this.FilePath!, this.ConfigName!), 250, 2);
            newConfig.ConfigurationUpdated = ConfigurationUpdated;

            // Disable events for this instance.
            DisposeEvents();

            // Call subscribers through the new config.
            newConfig.ConfigurationUpdated?.Invoke(newConfig);
        }
    }

    private void OnSave()
    {
        var parent = (TParentType)this;
        File.WriteAllText(FilePath!, JsonSerializer.Serialize(parent, SerializerOptions));
    }

    /* Utility */
    private static TParentType ReadFrom(string filePath, string configName)
    {
        var result = (File.Exists(filePath)
            ? JsonSerializer.Deserialize<TParentType>(File.ReadAllBytes(filePath), SerializerOptions)
            : new TParentType()) ?? new TParentType();

        result.Initialize(filePath, configName);
        return result;
    }
}