using System.ComponentModel;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <inheritdoc />
public class GameBananaPackageProviderFactory : IPackageProviderFactory
{
    /// <inheritdoc />
    public string ResolverId { get; } = "GBPackageProvider";

    /// <inheritdoc />
    public string FriendlyName { get; } = "GameBanana";

    /// <inheritdoc />
    public IDownloadablePackageProvider? GetProvider(PathTuple<ApplicationConfig> mod)
    {
        if (!this.TryGetConfiguration<GameBananaProviderConfig>(mod, out var gbConfig))
            return null;

        return new GameBananaPackageProvider(gbConfig!.GameId);
    }

    /// <inheritdoc />
    public bool TryGetConfigurationOrDefault(PathTuple<ApplicationConfig> mod, out object configuration)
    {
        var result = this.TryGetConfiguration<GameBananaProviderConfig>(mod, out var config);
        configuration = config ?? new GameBananaProviderConfig();
        return result;
    }

    /// <summary>
    /// Stores a configuration describing how to update mod using GameBanana.
    /// </summary>
    public class GameBananaProviderConfig : IConfig<GameBananaProviderConfig>
    {
        private const string Category = "GameBanana Settings";
        
        /// <summary>
        /// Type of the item on GameBanana, typically 'Mod'
        /// </summary>
        [Category(Category)]
        [Description("Id of the game on GameBanana, this is the last number in the URL to the game page.\n" +
                     "e.g. 6061 if your game URL is https://gamebanana.com/games/6061.")]
        public int GameId { get; set; } = 0;
    }
}