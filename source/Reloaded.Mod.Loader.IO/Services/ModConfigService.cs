using System.Collections.Generic;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO.Services
{
    /// <summary>
    /// Service which provides access to various mod configurations.
    /// </summary>
    public class ModConfigService : ConfigServiceBase<ModConfig>
    {
        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        public ModConfigService(LoaderConfig config, SynchronizationContext context = null)
        {
            Initialize(config.ModConfigDirectory, ModConfig.ConfigFileName, GetAllConfigs, context);
        }

        private List<PathTuple<ModConfig>> GetAllConfigs() => ModConfig.GetAllMods(base.ConfigDirectory);
    }
}
