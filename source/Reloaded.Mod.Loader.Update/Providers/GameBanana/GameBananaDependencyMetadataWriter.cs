using System;
using System.Collections.Generic;
using Reloaded.Mod.Interfaces.Utilities;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Sewer56.Update.Misc;

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Dependency metadata writer that appends information about GameBanana dependencies to the mod config.
/// </summary>
public class GameBananaDependencyMetadataWriter : IDependencyMetadataWriter
{
    /// <summary>
    /// Unique identifier used for storing dependency info in mod configurations.
    /// </summary>
    public const string PluginId = "GameBananaDependencies";

    /// <summary>
    /// ID of the resolver to copy dependency data from.
    /// </summary>
    public static readonly string ResolverId = Singleton<GameBananaUpdateResolverFactory>.Instance.ResolverId;

    /// <inheritdoc />
    public bool Update(ModConfig mod, IEnumerable<ModConfig> dependencies)
    {
        var newMetadata = new GameBananaResolverMetadata();

        // Get new metadata
        foreach (var dependency in dependencies)
        {
            if (dependency.PluginData.TryGetValue(ResolverId, out GameBananaUpdateResolverFactory.GameBananaConfig config))
                newMetadata.IdToConfigMap[dependency.ModId] = config;
        }
        
        var hasExisting = mod.PluginData.TryGetValue(PluginId, out GameBananaResolverMetadata metadata);

        // Require update if the metadata is new.
        if (!hasExisting && newMetadata.IdToConfigMap.Count > 0)
        {
            mod.PluginData[PluginId] = newMetadata;
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
                mod.PluginData.Remove(PluginId);
            else
                mod.PluginData[PluginId] = newMetadata;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Stores a configuration describing how to update mod using GameBanana.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public class GameBananaResolverMetadata : IConfig<GameBananaResolverMetadata>
    {
        /// <summary>
        /// Maps a list of individual ids to configurations.
        /// </summary>
        public Dictionary<string, GameBananaUpdateResolverFactory.GameBananaConfig> IdToConfigMap { get; private set; } = new (StringComparer.OrdinalIgnoreCase);
    }
}