using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// An interface that can be used to optionally return a resolver instance.
/// </summary>
public interface IUpdateResolverFactory
{
    /// <summary>
    /// The extractor to use with this resolver.
    /// </summary>
    IPackageExtractor Extractor { get; }

    /// <summary>
    /// Returns the unique ID of the resolver.
    /// </summary>
    string ResolverId { get; }

    /// <summary>
    /// Returns a friendly name for the resolver.
    /// </summary>
    string FriendlyName { get; }

    /// <summary>
    /// Migrates (if necessary) from an earlier version of the resolver.
    /// </summary>
    /// <param name="mod">The mod to check.</param>
    /// <param name="userConfig">User specific configuration for the given mod.</param>
    void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig);

    /// <summary>
    /// Returns an update resolver for the given modification. Otherwise returns null if this resolver cannot be used with the mod.
    /// </summary>
    /// <param name="mod">The mod to check.</param>
    /// <param name="data">Various configurations and general data provided to all the updaters.</param>
    /// <param name="userConfig">User specific configuration for the given mod.</param>
    /// <returns>A valid resolver if the mod can update through it, else null.</returns>
    IPackageResolver? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig, UpdaterData data);

    /// <summary>
    /// Gets the configuration for the resolver, or a default if one is not assigned.
    /// If the resolver has no configuration, the configuration is set to null.
    /// </summary>
    /// <param name="mod">The modification in question.</param>
    /// <param name="configuration">The returned configuration, can be current value, blank object (default) or null if not supported.</param>
    /// <returns>True if the mod had a configuration previously assigned, else false.</returns>
    bool TryGetConfigurationOrDefault(PathTuple<ModConfig> mod, out object configuration);
}

/// <summary>
/// Extension methods to make using resolver factories easier.
/// </summary>
public static class ResolverFactoryExtensions
{
    /// <summary>
    /// Attempts to get a configuration of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory to get configuration from.</param>
    /// <param name="mod">The mod config to get configuration from.</param>
    /// <param name="configuration">The returned configuration.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static bool TryGetConfiguration<T>(this IUpdateResolverFactory factory, PathTuple<ModConfig> mod, out T? configuration)
    {
        return mod.Config.PluginData.TryGetValue<T>(factory.ResolverId, out configuration);
    }

    /// <summary>
    /// Assigns a configuration of type T to the mod.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory from which the configuration is sourced from.</param>
    /// <param name="mod">The mod config to assign the configuration to.</param>
    /// <param name="value">The configuration value to set.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static void SetConfiguration<T>(this IUpdateResolverFactory factory, PathTuple<ModConfig> mod, T value)
    {
        mod.Config.PluginData[factory.ResolverId] = value;
    }

    /// <summary>
    /// Removes a configuration from the mod.
    /// </summary>
    /// <param name="factory">The factory from which the configuration is sourced from.</param>
    /// <param name="mod">The mod config to assign the configuration to.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static void RemoveConfiguration(this IUpdateResolverFactory factory, PathTuple<ModConfig> mod)
    {
        mod.Config.PluginData.Remove(factory.ResolverId);
    }
}