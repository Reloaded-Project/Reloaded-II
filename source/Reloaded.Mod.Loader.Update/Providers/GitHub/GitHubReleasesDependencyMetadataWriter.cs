namespace Reloaded.Mod.Loader.Update.Providers.GitHub;

/// <summary>
/// Dependency metadata writer that appends information about GitHub dependencies to the mod config.
/// </summary>
public class GitHubReleasesDependencyMetadataWriter : DependencyMetadataWriterBase<GitHubReleasesUpdateResolverFactory.GitHubConfig>
{
    /// <summary>
    /// Unique identifier used for storing dependency info in mod configurations.
    /// </summary>
    public const string PluginId = "GitHubDependencies";

    /// <summary>
    /// See <see cref="PluginId"/>.
    /// </summary>
    public override string GetPluginId() => PluginId;
    
    /// <summary>
    /// ID of the resolver to copy dependency data from.
    /// </summary>
    protected override string GetResolverId() => Singleton<GitHubReleasesUpdateResolverFactory>.Instance.ResolverId;
}