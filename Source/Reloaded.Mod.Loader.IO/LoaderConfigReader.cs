using System;
using System.IO;
using Newtonsoft.Json;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// Utility class that allows for finding the location of Reloaded II's installation.
    /// </summary>
    public class LoaderConfigReader
    {
        /// <summary>
        /// Name of Reloaded's folder inside AppData/Roaming.
        /// </summary>
        private const string ReloadedFolderName = "Reloaded-Mod-Loader-II";

        /// <summary>
        /// Location of the static configuration file, used to locate the mod loader install.
        /// </summary>
        private static readonly string StaticConfigFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{ReloadedFolderName}\\{LoaderConfig.ConfigFileName}";

        public string ConfigurationPath() => StaticConfigFilePath;

        /// <summary>
        /// Returns true if the configuration file exists, else false.
        /// </summary>
        public bool ConfigurationExists()
        {
            return File.Exists(StaticConfigFilePath);
        }

        /// <summary>
        /// Loads the mod loader configuration from disk.
        /// </summary>
        public LoaderConfig ReadConfiguration()
        {
            if (! File.Exists(StaticConfigFilePath))
                throw new FileNotFoundException($"Reloaded II's static config file path {StaticConfigFilePath} does not exist." +
                                    $" Reloaded II may not be installed.");

            string jsonFile = File.ReadAllText(StaticConfigFilePath);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return JsonConvert.DeserializeObject<LoaderConfig>(jsonFile, settings);
        }

        /// <summary>
        /// Writes a new mod loader configuration to disk.
        /// </summary>
        /// <param name="config">The new mod loader configuration to write.</param>
        public void WriteConfiguration(LoaderConfig config)
        {
            string directory = Path.GetDirectoryName(StaticConfigFilePath);
            if (! Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonFile = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(StaticConfigFilePath, jsonFile);
        }
    }
}
