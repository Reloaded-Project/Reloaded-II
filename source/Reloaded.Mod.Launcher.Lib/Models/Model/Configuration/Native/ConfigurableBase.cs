using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native;

/// <summary>
/// Base class for the configuration objects generated for native (non .NET) mods.
/// </summary>
/// <remarks>
/// Each configuration in a mod's <see cref="ModConfigSchema"/> becomes one derived
/// class, emitted by <see cref="ConfigTypeEmitter"/>. The derived class only holds
/// the settings as properties; this base class supplies what the launcher's
/// configuration dialog expects: display name, saving and file watching.
/// </remarks>
public abstract class ConfigurableBase : IUpdatableConfigurable
{
    /// <summary>
    /// Full path to the file storing the values of this configuration.
    /// </summary>
    [Browsable(false)]
    public string? FilePath { get; private set; }

    /// <summary>
    /// The name of the configuration, shown in the launcher dialog.
    /// </summary>
    [Browsable(false)]
    public string ConfigName { get; private set; } = "";

    /// <summary>
    /// Saves the current configuration to the hard disk.
    /// </summary>
    [Browsable(false)]
    public Action? Save { get; private set; }

    /// <summary>
    /// Automatically executed when the external configuration file is updated.
    /// </summary>
    [Browsable(false)]
    public event Action<IUpdatableConfigurable>? ConfigurationUpdated;

    /// <summary>
    /// Receives events on whenever the file is actively changed or updated.
    /// </summary>
    private FileSystemWatcher? ConfigWatcher { get; set; }

    /// <summary>
    /// Safety lock for when changed event gets raised twice on file save.
    /// </summary>
    private static object _readLock = new object();

    /// <summary>
    /// Initializes an instance after construction, arming the file watcher
    /// and save action.
    /// </summary>
    /// <param name="filePath">Full path to the file storing the values.</param>
    /// <param name="configName">Name displayed in the launcher dialog.</param>
    internal void Initialize(string filePath, string configName)
    {
        FilePath = filePath;
        ConfigName = configName;

        MakeConfigWatcher();
        Save = OnSave;
    }

    /// <summary>
    /// Halts the filesystem watcher and all events associated with this instance.
    /// </summary>
    public void DisposeEvents()
    {
        ConfigWatcher?.Dispose();
        ConfigWatcher = null;
        ConfigurationUpdated = null;
    }

    private void MakeConfigWatcher()
    {
        ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(FilePath!)!, Path.GetFileName(FilePath!)!);
        ConfigWatcher.Changed += (sender, e) => OnConfigurationUpdated();
        ConfigWatcher.EnableRaisingEvents = true;
    }

    private void OnConfigurationUpdated()
    {
        lock (_readLock)
        {
            // Note: External program might still be writing to file while this is being executed, so we need to keep retrying.
            var newConfig = ConfigIO.TryLoad(GetType(), FilePath!, ConfigName, 250, 2);
            if (newConfig == null)
                return;

            // Load and copy events, then disable events for this instance.
            newConfig.ConfigurationUpdated = ConfigurationUpdated;
            DisposeEvents();

            // Call subscribers through the new config.
            newConfig.ConfigurationUpdated?.Invoke(newConfig);
        }
    }

    private void OnSave() => ConfigIO.Save(this, FilePath!);
}

/// <summary>
/// Reads and writes the value files of native mod configurations.
/// </summary>
/// <remarks>
/// The file format is a flat JSON object mapping property names to values,
/// with enums stored as strings.
///
/// It is identical in shape to what the C# mod template writes, so C++ mods
/// can parse it with ease.
/// </remarks>
public static class ConfigIO
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>
    /// Stores the values of a configuration instance to disk.
    /// </summary>
    /// <param name="instance">Instance whose property values are written.</param>
    /// <param name="filePath">Full path of the file to write to.</param>
    public static void Save(object instance, string filePath)
    {
        var root = new JsonObject();
        foreach (var property in GetProperties(instance.GetType()))
        {
            var value = property.GetValue(instance);
            if (property.PropertyType.IsEnum)
            {
                root[property.Name] = value?.ToString();
                continue;
            }

            switch (Type.GetTypeCode(property.PropertyType))
            {
                case TypeCode.Boolean:
                    root[property.Name] = (bool)value!;
                    break;
                case TypeCode.Int32:
                    root[property.Name] = (int)value!;
                    break;
                case TypeCode.Single:
                    root[property.Name] = (float)value!;
                    break;
                case TypeCode.Double:
                    root[property.Name] = (double)value!;
                    break;
                case TypeCode.String:
                    root[property.Name] = (string?)value;
                    break;
            }
        }

        var directory = Path.GetDirectoryName(filePath);
        if (directory!.Length > 0)
            Directory.CreateDirectory(directory);

        File.WriteAllText(filePath, root.ToJsonString(SerializerOptions));
    }

    /// <summary>
    /// Applies the values from a file onto an instance.
    /// Properties missing from the file keep their current (default) values.
    /// </summary>
    /// <param name="instance">Instance to load the values into.</param>
    /// <param name="filePath">Full path of the file to read from.</param>
    /// <returns>
    /// True if loading succeeded; false if the file was missing or unreadable.
    /// </returns>
    public static bool Apply(object instance, string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var root = JsonNode.Parse(File.ReadAllText(filePath)) as JsonObject;
            if (root == null)
                return false;

            ApplyFromObject(instance, root);
            return true;
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new instance of the given configuration type with values
    /// loaded from disk.
    ///
    /// Missing or unreadable files yield an instance with the schema default values.
    /// </summary>
    /// <param name="type">Configuration type to create an instance of.</param>
    /// <param name="filePath">Full path of the file to load the values from.</param>
    /// <param name="configName">Name displayed in the launcher dialog.</param>
    /// <param name="timeout">Milliseconds to wait between read attempts.</param>
    /// <param name="retries">Number of attempts made to read the file.</param>
    /// <returns>
    /// The new instance, initialized and armed; values default when the file
    /// is missing or unreadable.
    /// </returns>
    public static ConfigurableBase Load(Type type, string filePath, string configName, int timeout = 0, int retries = 1)
    {
        var instance = (ConfigurableBase)Activator.CreateInstance(type)!;
        for (int x = 0; x < retries; x++)
        {
            if (Apply(instance, filePath))
                break;

            if (x + 1 < retries)
                Thread.Sleep(timeout);
        }

        instance.Initialize(filePath, configName);
        return instance;
    }

    /// <summary>
    /// Attempt to load a file from disk.
    /// Create a new instance of ConfigurableBase upon success.
    /// </summary>
    public static ConfigurableBase? TryLoad(Type type, string filePath, string configName, int timeout = 0, int retries = 1)
    {
        var instance = (ConfigurableBase)Activator.CreateInstance(type)!;
        for (int x = 0; x < retries; x++)
        {
            if (Apply(instance, filePath))
            {
                instance.Initialize(filePath, configName);
                return instance;
            }

            if (x + 1 < retries)
                Thread.Sleep(timeout);
        }

        return null;
    }

    private static void ApplyFromObject(object instance, JsonObject root)
    {
        foreach (var property in GetProperties(instance.GetType()))
        {
            if (!root.TryGetPropertyValue(property.Name, out var node) || node == null)
                continue;

            try
            {
                if (property.PropertyType.IsEnum)
                {
                    if (node.GetValueKind() == JsonValueKind.String && Enum.TryParse(property.PropertyType, node.GetValue<string>(), true, out var enumValue))
                        property.SetValue(instance, enumValue);
                }
                else
                {
                    switch (Type.GetTypeCode(property.PropertyType))
                    {
                        case TypeCode.Boolean:
                            property.SetValue(instance, node.GetValue<bool>());
                            break;
                        case TypeCode.Int32:
                            property.SetValue(instance, (int)node.GetValue<long>());
                            break;
                        case TypeCode.Single:
                            property.SetValue(instance, (float)node.GetValue<double>());
                            break;
                        case TypeCode.Double:
                            property.SetValue(instance, node.GetValue<double>());
                            break;
                        case TypeCode.String:
                            property.SetValue(instance, node.GetValue<string>());
                            break;
                    }
                }
            }
            catch (FormatException)
            {
                // Value doesn't fit the property; keep the current value.
            }
            catch (InvalidOperationException)
            {
                // Value has an unexpected JSON type; keep the current value.
            }
        }
    }

    /// <summary>
    /// Returns the editable settings declared by a generated configuration type.
    /// </summary>
    /// <param name="type">Configuration type to list the settings of.</param>
    /// <returns>
    /// The public read/write properties declared directly on the type.
    /// </returns>
    public static PropertyInfo[] GetProperties(Type type) => PropertyCache.GetOrAdd(type, static t => [.. t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.DeclaringType == t && p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)]);
}
