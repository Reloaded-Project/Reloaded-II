using System.Collections.Generic;
using Force.DeepCloner;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Resolvers;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Factory which returns the first appropriate resolver for a specific mod.
/// </summary>
public static class ResolverFactory
{    
    /// <summary>
    /// List of all supported factories in preference order.
    /// </summary>
    public static IResolverFactory[] All { get; } =
    {
        // Listed in order of preference.
        new NuGetResolverFactory(),
        new GitHubReleasesResolverFactory(),
        new GameBananaUpdateResolverFactory()
    };

    /// <summary>
    /// Returns the first appropriate resolver that can handle updating a mod.
    /// </summary>
    /// <param name="mod">The mod in question.</param>
    /// <param name="userConfig">Contains user configuration for this mod in question.</param>
    /// <param name="data">All data passed to the updater.</param>
    /// <returns>A resolver that can handle the mod, else null.</returns>
    public static AggregatePackageResolver? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig, UpdaterData data)
    {
        // Migrate first
        foreach (var factory in All)
            factory.Migrate(mod, userConfig);

        // Clone data preferences.
        data = data.DeepClone();
        if (userConfig.Config.AllowPrereleases.HasValue)
            data.CommonPackageResolverSettings.AllowPrereleases = userConfig.Config.AllowPrereleases.Value;

        data.CommonPackageResolverSettings.MetadataFileName = mod.Config.ReleaseMetadataFileName;

        // Create resolvers.
        var resolvers = new List<IPackageResolver>();
        foreach (var factory in All)
        {
            var resolver = factory.GetResolver(mod, userConfig, data);
            if (resolver != null)
                resolvers.Add(resolver);
        }

        return resolvers.Count > 0 ? new AggregatePackageResolver(resolvers) : null;
    }
}