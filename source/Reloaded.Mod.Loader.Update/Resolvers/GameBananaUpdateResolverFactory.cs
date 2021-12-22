using System.ComponentModel;
using System.IO;
using Reloaded.Mod.Interfaces.Utilities;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Resolvers.GameBanana;

namespace Reloaded.Mod.Loader.Update.Resolvers;

/// <summary>
/// Allows for updating of packages sourced from GameBanana.
/// </summary>
public class GameBananaUpdateResolverFactory : IUpdateResolverFactory
{
    /// <inheritdoc />
    public string ResolverId { get; } = "GameBanana";

    /// <inheritdoc />
    public string FriendlyName { get; } = "GameBanana";

    /// <inheritdoc/>
    public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig)
    {
        MigrateFromLegacyModConfig(mod);
    }

    /// <inheritdoc/>
    public IPackageResolver? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig, UpdaterData data)
    {
        if (!this.TryGetConfiguration<GameBananaConfig>(mod, out var gbConfig))
            return null;

        return new GameBananaUpdateResolver(new GameBananaResolverConfiguration()
        {
            ItemId = (int) gbConfig!.ItemId,
            ModType = gbConfig.ItemType
        }, data.CommonPackageResolverSettings);
    }

    /// <inheritdoc />
    public bool TryGetConfigurationOrDefault(PathTuple<ModConfig> mod, out object configuration)
    {
        var result = this.TryGetConfiguration<GameBananaConfig>(mod, out var config);
        configuration = config ?? new GameBananaConfig();
        return result;
    }
    
    private void MigrateFromLegacyModConfig(PathTuple<ModConfig> mod)
    {
        // Performs migration from legacy separate file config to integrated config.
        var configPath = GameBananaConfig.GetFilePath(GetModDirectory(mod));
        if (File.Exists(configPath))
        {
            var gbConfig = IConfig<GameBananaConfig>.FromPath(configPath);
            this.SetConfiguration(mod, gbConfig);
            mod.Save();
            IOEx.TryDeleteFile(configPath);
        }
    }

    private static string GetModDirectory(PathTuple<ModConfig> mod)
    {
        return Path.GetDirectoryName(mod.Path)!;
    }

    /// <summary>
    /// Stores a configuration describing how to update mod using GameBanana.
    /// </summary>
    public class GameBananaConfig : IConfig<GameBananaConfig>
    {
        private const string DefaultCategory = "GameBanana Settings";

        /// <summary/>
        public const string ConfigFileName = "ReloadedGamebananaUpdater.json";

        /// <summary/>
        public static string GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

        /// <summary>
        /// Type of the item on GameBanana, typically 'Mod'
        /// </summary>
        [Category(DefaultCategory)]
        [Description("Type of the item on GameBanana, typically 'Mod'.")]
        public string ItemType { get; set; } = "Mod";

        /// <summary>
        /// Id of the item on GameBanana, this is the last number in the URL to your mod page.
        /// </summary>
        [Category(DefaultCategory)]
        [Description("Id of the item on GameBanana, this is the last number in the URL to your mod page.\n" +
                     "e.g. 150115 if your mod URL is https://gamebanana.com/mods/150115.\n" +
                     "To get the URL to your mod page, you might need to upload your mod first as private.")]
        public long ItemId { get; set; }
    }
}