using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Interfaces.Utilities;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.Update;
using Sewer56.Update.Resolvers.GameBanana;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Resolves for missing GameBanana dependencies.
/// </summary>
public class GameBananaDependencyResolver : IDependencyResolver
{
    /// <inheritdoc />
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, ModConfig? modConfig = null, CancellationToken token = default)
    {
        // If no mod config is provided, we cannot resolve.
        if (modConfig == null)
            return new ModDependencyResolveResult() { NotFoundDependencies = { packageId }};

        // If no dependency data is available, return none.
        if (!modConfig.PluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out GameBananaResolverMetadata metadata))
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