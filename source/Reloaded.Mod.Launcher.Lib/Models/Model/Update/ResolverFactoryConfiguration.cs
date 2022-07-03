namespace Reloaded.Mod.Launcher.Lib.Models.Model.Update;

/// <summary>
/// Model for an item to be configured.
/// </summary>
public class ResolverFactoryConfiguration : ObservableObject
{
    /// <summary>
    /// If the item is enabled or not.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The configuration to edit. Can be null.
    /// Use reflection to configure this item.
    /// </summary>
    public object Configuration { get; set; } = null!;

    /// <summary>
    /// The factory responsible for handling the configuration.
    /// </summary>
    public IUpdateResolverFactory Factory { get; set; } = null!;

    private ResolverFactoryConfiguration() { }

    /// <summary>
    /// Tries to create a configuration for the resolver.
    /// </summary>
    /// <param name="factory">The factory used to get and set the configuration.</param>
    /// <param name="mod">The mod to assign the configuration to.</param>
    /// <returns>The configuration.</returns>
    public static ResolverFactoryConfiguration? TryCreate(IUpdateResolverFactory factory, PathTuple<ModConfig> mod)
    {
        var isEnabled = factory.TryGetConfigurationOrDefault(mod, out var configuration);
        if (configuration == null)
            return null;

        return new ResolverFactoryConfiguration()
        {
            Configuration = configuration,
            IsEnabled = isEnabled,
            Factory = factory
        };
    }
}