using System.Collections.Generic;
using System.Threading;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Config
{
    [Equals(DoNotAddEqualityOperators = true, DoNotAddGetHashCode = true)]
    public class ModUserConfig : ObservableObject, IConfig<ModUserConfig>, IModUserConfig
    {
        public const string ConfigFileName = "ModUserConfig.json";

        /* Class Members */
        public string ModId { get; set; }

        public Dictionary<string, object> PluginData { get; set; }

        public bool IsUniversalMod { get; set; }

        /*
           ---------
           Utilities
           --------- 
        */

        /// <summary>
        /// Finds all mod user configs on the filesystem, parses them and returns a list of all mod user configs.
        /// </summary>
        /// <param name="configDirectory">(Optional) Directory containing all of the applications.</param>
        /// <param name="token">Optional token used to cancel the operation.</param>
        public static List<PathTuple<ModUserConfig>> GetAllUserConfigs(string configDirectory = null, CancellationToken token = default)
        {
            if (configDirectory == null)
                configDirectory = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath).ApplicationConfigDirectory;

            return ConfigReader<ModUserConfig>.ReadConfigurations(configDirectory, ConfigFileName, token, 2);
        }
    }
}
