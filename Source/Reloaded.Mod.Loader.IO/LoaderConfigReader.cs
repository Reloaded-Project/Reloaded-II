using System;
using System.IO;
using System.Text.Json;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// Utility class that allows for finding the location of Reloaded II's installation.
    /// </summary>
    public class LoaderConfigReader
    {
        public static string ConfigurationPath() => Paths.LauncherConfigPath;

        /// <summary>
        /// Returns true if the configuration file exists, else false.
        /// </summary>
        public static bool ConfigurationExists()
        {
            return File.Exists(Paths.LauncherConfigPath);
        }

        /// <summary>
        /// Loads the mod loader configuration from disk.
        /// </summary>
        public static LoaderConfig ReadConfiguration()
        {
            var config = new LoaderConfig();
            if (ConfigurationExists())
            {
                string jsonFile = File.ReadAllText(Paths.LauncherConfigPath);
                config = JsonSerializer.Deserialize<LoaderConfig>(jsonFile);
            }

            config.SanitizeConfig();
            return config;
        }

        /// <summary>
        /// Writes a new mod loader configuration to disk.
        /// </summary>
        /// <param name="config">The new mod loader configuration to write.</param>
        public static void WriteConfiguration(LoaderConfig config)
        {
            string directory = Path.GetDirectoryName(Paths.LauncherConfigPath);
            if (! Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonFile = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(Paths.LauncherConfigPath, jsonFile);
        }
    }
}
