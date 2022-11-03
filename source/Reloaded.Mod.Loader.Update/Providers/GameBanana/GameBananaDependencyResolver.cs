namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Resolves for missing GameBanana dependencies.
/// </summary>
public class GameBananaDependencyResolver : IDependencyResolver
{
    /// <inheritdoc />
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default)
    {
        // If no mod config is provided, we cannot resolve.
        if (pluginData == null)
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId }};

        // If no dependency data is available, return none.
        if (!pluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out DependencyResolverMetadata<GameBananaUpdateResolverFactory.GameBananaConfig> metadata))
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId } };

        // Try to get configuration for update.
        if (!metadata.IdToConfigMap.TryGetValue(packageId, out var gbConfig))
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId } };

        var result   = new ModDependencyResolveResult();
        var resolver = new GameBananaUpdateResolver(new GameBananaResolverConfiguration()
        {
            ItemId = (int)gbConfig.Config.ItemId,
            ModType = gbConfig.Config.ItemType
        }, new CommonPackageResolverSettings() { MetadataFileName = gbConfig.ReleaseMetadataName });

        await resolver.InitializeAsync();

        result.FoundDependencies.Add(new UpdateDownloadablePackage(resolver)
        {
            Id = packageId
        });

        return result;
    }
}