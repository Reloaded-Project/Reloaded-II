using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update.Providers.GitHub;

/// <summary>
/// Resolves for missing GitHub dependencies.
/// </summary>
public class GitHubDependencyResolver : IDependencyResolver
{
    /// <inheritdoc />
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default)
    {        
        // If no mod config is provided, we cannot resolve.
        if (pluginData == null)
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId } };

        // If no dependency data is available, return none.
        if (!pluginData.TryGetValue(GitHubReleasesDependencyMetadataWriter.PluginId, out DependencyResolverMetadata<GitHubReleasesUpdateResolverFactory.GitHubConfig> metadata))
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId } };

        // Try to get configuration for update.
        if (!metadata.IdToConfigMap.TryGetValue(packageId, out var gitConfig))
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId } };

        var result = new ModDependencyResolveResult();
        var resolver = new GitHubReleaseResolver(new GitHubResolverConfiguration()
        {
            UserName = gitConfig.Config.UserName,
            RepositoryName = gitConfig.Config.RepositoryName,
            LegacyFallbackPattern = gitConfig.Config.AssetFileName,
            InheritVersionFromTag = gitConfig.Config.UseReleaseTag
        }, new CommonPackageResolverSettings() { MetadataFileName = gitConfig.ReleaseMetadataName });

        await ((IPackageResolver)resolver).InitializeAsync();
        result.FoundDependencies.Add(new UpdateDownloadablePackage(resolver)
        {
            Id = packageId
        });

        return result;
    }
}