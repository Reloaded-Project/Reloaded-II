namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Dependency metadata writer that appends information about GameBanana dependencies to the mod config.
/// </summary>
public class GameBananaDependencyMetadataWriter : DependencyMetadataWriterBase<GameBananaUpdateResolverFactory.GameBananaConfig>
{
    /// <summary>
    /// Unique identifier used for storing dependency info in mod configurations.
    /// </summary>
    public const string PluginId = "GameBananaDependencies";

    /// <summary>
    /// See <see cref="PluginId"/>.
    /// </summary>
    public override string GetPluginId() => PluginId;
    
    /// <summary>
    /// ID of the resolver to copy dependency data from.
    /// </summary>
    protected override string GetResolverId() => Singleton<GameBananaUpdateResolverFactory>.Instance.ResolverId;
}