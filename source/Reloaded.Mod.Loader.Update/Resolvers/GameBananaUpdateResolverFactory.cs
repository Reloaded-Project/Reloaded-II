using System.IO;
using Reloaded.Mod.Interfaces.Utilities;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Resolvers.GameBanana;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    public class GameBananaUpdateResolverFactory : IResolverFactory
    {
        public string ResolverId { get; } = "GameBanana";

        /// <inheritdoc/>
        public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig)
        {
            MigrateFromLegacyModConfig(mod);
        }

        /// <inheritdoc/>
        public IPackageResolver GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig, UpdaterData data)
        {
            if (!mod.Config.PluginData.TryGetValue<GameBananaConfig>(ResolverId, out var gbConfig))
                return null;

            return new GameBananaUpdateResolver(new GameBananaResolverConfiguration()
            {
                ItemId = (int) gbConfig.ItemId,
                ModType = gbConfig.ItemType
            }, data.CommonPackageResolverSettings);
        }

        private void MigrateFromLegacyModConfig(PathTuple<ModConfig> mod)
        {
            // Performs migration from legacy separate file config to integrated config.
            var configPath = GameBananaConfig.GetFilePath(GetModDirectory(mod));
            if (File.Exists(configPath))
            {
                var gbConfig = IConfig<GameBananaConfig>.FromPath(configPath);
                mod.Config.PluginData[ResolverId] = gbConfig;
                mod.Save();
                IOEx.TryDeleteFile(configPath);
            }
        }

        private static string GetModDirectory(PathTuple<ModConfig> mod)
        {
            return Path.GetDirectoryName(mod.Path);
        }

        public class GameBananaConfig : IConfig<GameBananaConfig>
        {
            public const string ConfigFileName = "ReloadedGamebananaUpdater.json";

            public static string GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public string ItemType { get; set; }

            public long ItemId { get; set; }
        }
    }
}
