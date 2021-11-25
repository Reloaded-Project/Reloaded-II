using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Services
{
    /// <summary>
    /// Service which provides access to various mod configurations.
    /// </summary>
    public class ModUserConfigService : ConfigServiceBase<ModUserConfig>
    {
        /// <summary>
        /// All mod user configs by their unique ID.
        /// </summary>
        public Dictionary<string, PathTuple<ModUserConfig>> ItemsById { get; private set; } = new Dictionary<string, PathTuple<ModUserConfig>>(StringComparer.OrdinalIgnoreCase);

        private ModConfigService _modConfigService;

        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        /// <param name="modConfigService">Allows to receive notifications on mods being deleted/created.</param>
        public ModUserConfigService(LoaderConfig config, SynchronizationContext context, ModConfigService modConfigService)
        {
            this.OnAddItem    += OnAddItemHandler;
            this.OnRemoveItem += OnRemoveItemHandler;

            Initialize(config.ModUserConfigDirectory, ModUserConfig.ConfigFileName, GetAllConfigs, context);

            _modConfigService = modConfigService;
            modConfigService.OnAddItem    += CreateUserConfigOnNewConfigCreated;
            modConfigService.OnRemoveItem += DeleteUserConfigOnConfigDeleted;

            CreateConfigsForModsWithoutAny();
        }

        private void OnRemoveItemHandler(PathTuple<ModUserConfig> obj) => ItemsById.Remove(obj.Config.ModId);

        private void OnAddItemHandler(PathTuple<ModUserConfig> obj) => ItemsById[obj.Config.ModId] = obj;

        private void CreateConfigsForModsWithoutAny()
        {
            foreach (var mod in _modConfigService.Items)
            {
                if (ItemsById.ContainsKey(mod.Config.ModId) || File.Exists(GetUserConfigPathForMod(mod.Config.ModId)))
                    continue;

                CreateUserConfigOnNewConfigCreated(mod);
            }
        }

        private List<PathTuple<ModUserConfig>> GetAllConfigs() => ModUserConfig.GetAllUserConfigs(base.ConfigDirectory);

        private void CreateUserConfigOnNewConfigCreated(PathTuple<ModConfig> tuple)
        {
            // Make folder path and save folder.
            string modDirectory = GetUserConfigFolderForMod(tuple.Config.ModId);
            Directory.CreateDirectory(modDirectory);

            // Save Config
            IConfig<ModUserConfig>.ToPath(new ModUserConfig() { ModId = tuple.Config.ModId }, GetUserConfigPathForMod(tuple.Config.ModId));
        }

        private void DeleteUserConfigOnConfigDeleted(PathTuple<ModConfig> tuple) => IOEx.TryDeleteDirectory(Path.GetDirectoryName(tuple.Path), true);

        private string GetUserConfigFolderForMod(string modId) => Path.Combine(ConfigDirectory, IOEx.ForceValidFilePath(modId));

        private string GetUserConfigPathForMod(string modId) => Path.Combine(GetUserConfigFolderForMod(modId), ModUserConfig.ConfigFileName);
    }
}
