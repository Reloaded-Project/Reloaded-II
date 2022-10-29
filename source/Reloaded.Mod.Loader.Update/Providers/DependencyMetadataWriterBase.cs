namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Base class for quick dependency metadata writers.
/// </summary>
public abstract class DependencyMetadataWriterBase<TConfig> : IDependencyMetadataWriter where TConfig : new()
{
    /// <summary>
    /// Gets the resolver id for this config.
    /// </summary>
    public abstract string GetPluginId();

    /// <summary>
    /// Gets the resolver id for <typeparamref name="TConfig"/>.
    /// </summary>
    protected abstract string GetResolverId();

    /// <inheritdoc />
    public bool Update(ModConfig mod, IEnumerable<ModConfig> dependencies)
    {
        var newMetadata = new DependencyResolverMetadata<TConfig>();

        // Get new metadata
        foreach (var dependency in dependencies)
        {
            if (dependency.PluginData.TryGetValue(GetResolverId(), out TConfig config))
            {
                newMetadata.IdToConfigMap[dependency.ModId] = new DependencyResolverItem<TConfig>()
                {
                    Config = config,
                    ReleaseMetadataName = dependency.ReleaseMetadataFileName
                };
            }
        }

        var hasExisting = mod.PluginData.TryGetValue(GetPluginId(), out DependencyResolverMetadata<TConfig> metadata);

        // Require update if the metadata is new.
        if (!hasExisting && newMetadata.IdToConfigMap.Count > 0)
        {
            mod.PluginData[GetPluginId()] = newMetadata;
            return true;
        }

        // Otherwise if it has changed.
        if (!newMetadata.Equals(metadata))
        {
            // If there isn't any to begin with, nothing changed.
            if (!hasExisting)
                return false;

            // Remove if no items, else update.
            if (newMetadata.IdToConfigMap.Count <= 0)
                mod.PluginData.Remove(GetPluginId());
            else
                mod.PluginData[GetPluginId()] = newMetadata;

            return true;
        }

        return false;
    }
}


/// <summary>
/// Stores a configuration describing how to update a mod.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public class DependencyResolverMetadata<TConfig> : IConfig<DependencyResolverMetadata<TConfig>> where TConfig : new()
{
    /// <summary>
    /// Maps a list of individual ids to configurations.
    /// </summary>
    public Dictionary<string, DependencyResolverItem<TConfig>> IdToConfigMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Reflection-less JSON
    /// <inheritdoc />
    public static JsonTypeInfo<DependencyResolverMetadata<TConfig>> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = false;
        return null!;
    }
    
    /// <inheritdoc />
    public JsonTypeInfo<DependencyResolverMetadata<TConfig>> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
}

/// <summary/>
[Equals(DoNotAddEqualityOperators = true)]
public class DependencyResolverItem<TConfig> where TConfig : new()
{
    /// <summary>
    /// The configuration associated with this item.
    /// </summary>
    public TConfig Config { get; set; } = new();

    /// <summary>
    /// Name of the release metadata file.
    /// </summary>
    public string ReleaseMetadataName { get; set; } = Singleton<ReleaseMetadata>.Instance.GetDefaultFileName();

    /// <summary/>
    public DependencyResolverItem() { }
}