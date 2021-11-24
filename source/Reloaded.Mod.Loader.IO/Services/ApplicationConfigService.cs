using System.Collections.Generic;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO.Services
{
    /// <summary>
    /// Service which provides access to various application configurations.
    /// </summary>
    public class ApplicationConfigService : ConfigServiceBase<ApplicationConfig>
    {
        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        public ApplicationConfigService(LoaderConfig config, SynchronizationContext context = null)
        {
            Initialize(config.ApplicationConfigDirectory, ApplicationConfig.ConfigFileName, GetAllConfigs, context);
        }

        private List<PathTuple<ApplicationConfig>> GetAllConfigs() => ApplicationConfig.GetAllApplications(base.ConfigDirectory);
    }
}