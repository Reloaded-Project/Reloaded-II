using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;
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
    public class ConfigReader<TConfigType> : IConfigLoader<TConfigType> where TConfigType : IConfig
    {
        /* Documentation: See IConfigLoader. */
        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        /* Interface */
        public List<PathGenericTuple<TConfigType>> ReadConfigurations(string directory, string fileName) => ReadConfigurations(directory, fileName, default);
        public void WriteConfigurations(PathGenericTuple<TConfigType>[] configurations) => WriteConfigurations(configurations, default);

        /* Implementation with Cancellation. */

        public List<PathGenericTuple<TConfigType>> ReadConfigurations(string directory, string fileName, CancellationToken token)
        {
            // Get all config files to load.
            string[] configurationPaths = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);

            // Configurations to be returned
            var configurations = new List<PathGenericTuple<TConfigType>>(configurationPaths.Length);
            foreach (string configurationPath in configurationPaths)
                if (! token.IsCancellationRequested)
                    configurations.Add( new PathGenericTuple<TConfigType>(configurationPath, ReadConfiguration(configurationPath)) );

            return configurations;
        }

        /* Excluding because path and WriteConfiguration are tested. */
        public void WriteConfigurations(PathGenericTuple<TConfigType>[] configurations, CancellationToken token)
        {
            foreach (var configuration in configurations)
                if (!token.IsCancellationRequested)
                    WriteConfiguration(configuration.Path, configuration.Object);
        }

        public TConfigType ReadConfiguration(string path)
        {
            string jsonFile = File.ReadAllText(path);
            return JsonSerializer.Parse<TConfigType>(jsonFile, Options);
        }

        public void WriteConfiguration(string path, TConfigType config)
        {
            string fullPath = Path.GetFullPath(path);
            string directoryOfPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonSerializer.ToString(config, Options);
            File.WriteAllText(path, jsonFile);
        }
    }
}
