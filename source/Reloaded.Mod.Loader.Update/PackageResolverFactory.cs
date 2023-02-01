using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Builds an <see cref="AggregatePackageResolver"/> using all configured individual resolver factories.
/// </summary>
public static class PackageResolverFactory
{    
    /// <summary>
    /// List of all supported factories in preference order.
    /// </summary>
    public static IUpdateResolverFactory[] All { get; private set; } =
    {
        // Listed in order of preference.
        // Note that AggregatePackageResolver will pick based on smallest download size first, then use this order.
        new NuGetUpdateResolverFactory(),
        new GitHubReleasesUpdateResolverFactory(),
        new GameBananaUpdateResolverFactory()
    };

    /// <summary>
    /// Returns the first appropriate resolver that can handle updating a mod.
    /// </summary>
    /// <param name="mod">The mod in question.</param>
    /// <param name="userConfig">Contains user configuration for this mod in question.</param>
    /// <param name="data">All data passed to the updater.</param>
    /// <returns>A resolver that can handle the mod, else null.</returns>
    public static AggregatePackageResolverEx? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig, UpdaterData data)
    {
        // Migrate first
        foreach (var factory in All)
            factory.Migrate(mod, userConfig);

        // Clone data preferences.
        data = data.DeepClone();
        if (userConfig?.Config.AllowPrereleases != null)
            data.CommonPackageResolverSettings.AllowPrereleases = userConfig.Config.AllowPrereleases.Value;

        data.CommonPackageResolverSettings.MetadataFileName = mod.Config.ReleaseMetadataFileName;

        // Create resolvers.
        var resolvers  = new List<IPackageResolver>();
        var extractors = new Dictionary<IPackageResolver, IPackageExtractor>();
        foreach (var factory in All)
        {
            var resolver = factory.GetResolver(mod, userConfig, data);
            if (resolver != null)
            {
                resolvers.Add(resolver);
                extractors[resolver] = factory.Extractor;
            }
        }

        return resolvers.Count > 0 ? new AggregatePackageResolverEx(resolvers, extractors) : null;
    }

    /// <summary>
    /// Returns true if any resolver is configured to save updates for this mod.
    /// </summary>
    /// <param name="mod">The mod to be served updates for.</param>
    /// <returns>Resolver configuration.</returns>
    public static bool HasAnyConfiguredResolver(PathTuple<ModConfig> mod)
    {
        foreach (var factory in All)
        {
            if (factory.TryGetConfigurationOrDefault(mod, out _))
                return true;
        }

        return false;
    }

    /// <summary>
    /// TEST USE ONLY.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetResolverFactories(IUpdateResolverFactory[] factories)
    {
        All = factories;
    }
}