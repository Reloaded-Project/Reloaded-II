using System;
using System.IO;
using System.Text.Json.Serialization;
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

        public static string ConfigurationPath() => StaticConfigFilePath;

        /// <summary>
        /// Returns true if the configuration file exists, else false.
        /// </summary>
        public static bool ConfigurationExists()
        {
            return File.Exists(StaticConfigFilePath);
        }

        /// <summary>
        /// Loads the mod loader configuration from disk.
        /// </summary>
        public static LoaderConfig ReadConfiguration()
        {
            if (! File.Exists(StaticConfigFilePath))
                throw new FileNotFoundException($"Reloaded II's static config file path {StaticConfigFilePath} does not exist." +
                                    $" Reloaded II may not be installed.");

            string jsonFile = File.ReadAllText(StaticConfigFilePath);
            var config = JsonSerializer.Parse<LoaderConfig>(jsonFile);
            config.ResetMissingDirectories();
            return config;
        }

        /// <summary>
        /// Writes a new mod loader configuration to disk.
        /// </summary>
        /// <param name="config">The new mod loader configuration to write.</param>
        public static void WriteConfiguration(LoaderConfig config)
        {
            string directory = Path.GetDirectoryName(StaticConfigFilePath);
            if (! Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonFile = JsonSerializer.ToString(config, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(StaticConfigFilePath, jsonFile);
        }
    }
}
