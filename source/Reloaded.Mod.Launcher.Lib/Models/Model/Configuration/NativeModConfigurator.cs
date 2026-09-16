namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration;

/// <summary>
/// Configurator for native (non .NET) mods that declare their settings through a <c>ConfigSchema.json</c> file.
/// Use the same interface as a C# mod's configurator.
/// </summary>
public class NativeModConfigurator : IConfiguratorV3
{
    private string _schemaPath;
    private string _modDirectory = "";
    private string? _configDirectory;
    private ConfiguratorContext _context;

    /// <summary>
    /// Creates a configurator for a mod folder containing <see cref="NativeModConfigSchema.SchemaFileName"/>.
    /// </summary>
    /// <param name="modDirectory">Full path to the folder containing the mod.</param>
    public NativeModConfigurator(string modDirectory)
    {
        _modDirectory = modDirectory;
        _schemaPath = Path.Combine(modDirectory, NativeModConfigSchema.SchemaFileName);
    }

    /// <inheritdoc />
    public void SetModDirectory(string modDirectory)
    {
        _modDirectory = modDirectory;
        _schemaPath = Path.Combine(modDirectory, NativeModConfigSchema.SchemaFileName);
    }

    /// <inheritdoc />
    public IConfigurable[] GetConfigurations()
    {
        var schema = NativeModConfigSchema.Load(_modDirectory);
        var configDirectory = _configDirectory ?? _modDirectory;

        // Include the file's last write time in the cache key, such that mod updates invalidate emitted types.
        var lastWrite = File.GetLastWriteTimeUtc(_schemaPath).Ticks.ToString();
        var result = new List<IConfigurable>(schema.Configurations.Count);
        foreach (var configuration in schema.Configurations)
        {
            var cacheKey = $"{_modDirectory}|{configuration.FileName}|{lastWrite}";
            var instance = NativeConfigTypeEmitter.CreateInstance(configuration, cacheKey);

            var valuesPath = Path.Combine(configDirectory, configuration.FileName);
            NativeConfigIO.Apply(instance, valuesPath);
            instance.Initialize(valuesPath, configuration.DisplayName ?? Path.GetFileNameWithoutExtension(configuration.FileName));
            result.Add(instance);
        }

        return result.ToArray();
    }

    /// <inheritdoc />
    public bool TryRunCustomConfiguration() => false;

    /// <inheritdoc />
    public void Migrate(string oldDirectory, string newDirectory) => TryMigrate(oldDirectory, newDirectory);

    /// <summary>
    /// Moves value files left behind in an old directory over to a new one.
    /// On failure the files already moved return to
    /// the old directory and the reason is in <see cref="MigrationError"/>.
    /// </summary>
    /// <param name="oldDirectory">The old mod config directory, usually the mod folder.</param>
    /// <param name="newDirectory">The new mod config directory, usually the user config folder.</param>
    public bool TryMigrate(string oldDirectory, string newDirectory)
    {
        MigrationError = null;
        var moved = new List<(string OldPath, string NewPath)>();
        try
        {
            var schema = NativeModConfigSchema.Load(_modDirectory);
            Directory.CreateDirectory(newDirectory);
            foreach (var configuration in schema.Configurations)
            {
                var oldPath = Path.Combine(oldDirectory, configuration.FileName);
                var newPath = Path.Combine(newDirectory, configuration.FileName);
                if (File.Exists(oldPath) && !File.Exists(newPath))
                {
                    File.Move(oldPath, newPath);
                    moved.Add((oldPath, newPath));
                }
            }

            return true;
        }
        catch (Exception e)
        {
            MigrationError = e;

            // The caller keeps using the old directory, so put back what was moved.
            for (int x = moved.Count - 1; x >= 0; x--)
            {
                try
                {
                    if (!File.Exists(moved[x].OldPath))
                        File.Move(moved[x].NewPath, moved[x].OldPath);
                }
                catch (Exception)
                {
                    // MigrationError above holds the real cause.
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Exception of the last failed migration, if any.
    /// </summary>
    public Exception? MigrationError { get; private set; }

    /// <inheritdoc />
    public void SetConfigDirectory(string configDirectory) => _configDirectory = configDirectory;

    /// <inheritdoc />
    public void SetContext(in ConfiguratorContext context) => _context = context;
}
