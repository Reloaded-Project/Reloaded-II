using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// Generic class for loading various configuration files such as mod and game configurations.
    /// </summary>
    /// <typeparam name="TConfigType">
    ///     A json config class that implements interface <see cref="IConfig"/>.
    ///     Examples: <see cref="ModConfig"/>, <see cref="ApplicationConfig"/>.
    /// </typeparam>
    public class ConfigReader<TConfigType> where TConfigType : IConfig, new()
    {
        /* Documentation: See IConfigLoader. */
        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        /* Implementation with Cancellation. */

        /// <summary>
        /// Recursively searches all directories under the provided absolute directory paths
        /// and loads all configurations with a matching file name.
        /// </summary>
        /// <param name="directory">The absolute path of the directory from which to load all configurations from.</param>
        /// <param name="fileName">The name of the file to load. The filename can contain wildcards * but not regex.</param>
        /// <param name="token">Cancels the task if necessary.</param>
        /// <param name="maxDepth">Maximum depth (inclusive) in directories to get files from where 1 is only this directory.</param>
        /// <param name="minDepth">Minimum depth (inclusive) in directories to get files from where 1 is this directory.</param>
        /// <returns>Tuples containing the path the configurations was loaded from and the corresponding config class.</returns>
        public List<PathGenericTuple<TConfigType>> ReadConfigurations(string directory, string fileName, CancellationToken token = default, int maxDepth = 1, int minDepth = 1)
        {
            // Get all config files to load.
            var configurationPaths = Utility.GetFilesEx(directory, fileName, maxDepth, minDepth);

            // Configurations to be returned
            var configurations = new List<PathGenericTuple<TConfigType>>(configurationPaths.Count);
            foreach (string configurationPath in configurationPaths)
            {
                if (token.IsCancellationRequested)
                    return configurations;

                if (TryReadConfiguration(configurationPath, out var config))
                    configurations.Add(new PathGenericTuple<TConfigType>(configurationPath, config));
            }

            return configurations;
        }

        /* Excluding because path and WriteConfiguration are tested. */

        /// <summary>
        /// Writes all file path and configurations tuples returned from <see cref="ReadConfigurations"/> back to disk.
        /// </summary>
        /// <param name="configurations">List of file path and config class tuples to write to disk.</param>
        /// <param name="token">Cancels the task if necessary.</param>
        public void WriteConfigurations(PathGenericTuple<TConfigType>[] configurations, CancellationToken token = default)
        {
            foreach (var configuration in configurations)
                if (!token.IsCancellationRequested)
                    WriteConfiguration(configuration.Path, configuration.Object);
        }

        /// <summary>
        /// Loads a given mod configurations from an absolute file path.
        /// </summary>
        /// <param name="path">The absolute file path of the config file.</param>
        /// <param name="value">The obtained configuration.</param>
        public bool TryReadConfiguration(string path, out TConfigType value)
        {
            try
            {
                value = ReadConfiguration(path);
                return true;
            }
            catch (Exception)
            {
                value = new TConfigType();
                return false;
            }
        }

        /// <summary>
        /// Loads a given mod configurations from an absolute file path.
        /// </summary>
        /// <param name="path">The absolute file path of the config file.</param>
        public TConfigType ReadConfiguration(string path)
        {
            string jsonFile = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<TConfigType>(jsonFile, Options);
            result.SetNullValues();
            return result;
        }

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="path">The absolute path to write the configurations file to.</param>
        /// <param name="config">The mod configurations to commit to file.</param>
        public void WriteConfiguration(string path, TConfigType config)
        {
            string fullPath = Path.GetFullPath(path);
            string directoryOfPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonSerializer.Serialize(config, Options);
            File.WriteAllText(fullPath, jsonFile);
        }
    }
}
