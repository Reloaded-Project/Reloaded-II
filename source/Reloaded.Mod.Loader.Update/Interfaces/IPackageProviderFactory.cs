namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// An interface that can be used to optionally return a <see cref="IDownloadablePackageProvider"/> instance for an individual application.
/// </summary>
public interface IPackageProviderFactory
{
    /// <summary>
    /// Returns the unique ID of the resolver.
    /// </summary>
    string ResolverId { get; }

    /// <summary>
    /// Returns a friendly name for the resolver.
    /// </summary>
    string FriendlyName { get; }
    
    /// <summary>
    /// Returns a package provider for this individual mod. Otherwise returns null if not associated with the application.
    /// </summary>
    /// <param name="mod">The application to check.</param>
    /// <returns>A valid package provider for the application.</returns>
    IDownloadablePackageProvider? GetProvider(PathTuple<ApplicationConfig> mod);

    /// <summary>
    /// Gets the configuration for the provider, or a default if one is not assigned.
    /// If the provider has no configuration, the configuration is set to null.
    /// </summary>
    /// <param name="mod">The application in question.</param>
    /// <param name="configuration">The returned configuration, can be current value, blank object (default) or null if not supported.</param>
    /// <returns>True if the application had a configuration previously assigned, else false.</returns>
    bool TryGetConfigurationOrDefault(PathTuple<ApplicationConfig> mod, out object configuration);
}

/// <summary>
/// Extension methods to make using resolver factories easier.
/// </summary>
public static class PackageResolverFactoryExtensions
{
    /// <summary>
    /// Attempts to get a configuration of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory to get configuration from.</param>
    /// <param name="application">The application config to get configuration from.</param>
    /// <param name="configuration">The returned configuration.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static bool TryGetConfiguration<T>(this IPackageProviderFactory factory, PathTuple<ApplicationConfig> application, out T? configuration)
    {
        return application.Config.PluginData.TryGetValue<T>(factory.ResolverId, out configuration);
    }

    /// <summary>
    /// Assigns a configuration of type T to the application.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory from which the configuration is sourced from.</param>
    /// <param name="application">The application config to assign the configuration to.</param>
    /// <param name="value">The configuration value to set.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static void SetConfiguration<T>(this IPackageProviderFactory factory, PathTuple<ApplicationConfig> application, T value)
    {
        application.Config.PluginData[factory.ResolverId] = value;
    }

    /// <summary>
    /// Removes a configuration from the application.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory from which the configuration is sourced from.</param>
    /// <param name="application">The application config to assign the configuration to.</param>
    /// <returns>Whether the configuration was found or not.</returns>
    public static void RemoveConfiguration<T>(this IPackageProviderFactory factory, PathTuple<ApplicationConfig> application)
    {
        application.Config.PluginData.Remove(factory.ResolverId);
    }
}