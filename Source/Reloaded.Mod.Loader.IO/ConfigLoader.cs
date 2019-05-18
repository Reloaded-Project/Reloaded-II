using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Reloaded.Mod.Loader.IO.Config;
using Newtonsoft.Json;
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
    public class ConfigLoader<TConfigType> : IConfigLoader<TConfigType> where TConfigType : IConfig
    {
        /* Documentation: See IConfigLoader. */

        public List<PathGenericTuple<TConfigType>> ReadConfigurations(string directory, string fileName)
        {
            // Get all config files to load.
            string[] configurationPaths = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);

            // Configurations to be returned
            var configurations = new List<PathGenericTuple<TConfigType>>(configurationPaths.Length);
            foreach (string configurationPath in configurationPaths)
                configurations.Add( new PathGenericTuple<TConfigType>(configurationPath, ReadConfiguration(configurationPath)) );

            return configurations;
        }

        /* Excluding because path and WriteConfiguration are tested. */
        [ExcludeFromCodeCoverage]
        public void WriteConfigurations(PathGenericTuple<TConfigType>[] configurations)
        {
            foreach (var configuration in configurations)
                WriteConfiguration(configuration.Path, configuration.Object);
        }

        public TConfigType ReadConfiguration(string path)
        {
            string jsonFile = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<TConfigType>(jsonFile);
        }

        public void WriteConfiguration(string path, TConfigType modConfig)
        {
            string directoryOfPath = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonConvert.SerializeObject(modConfig, Formatting.Indented);
            File.WriteAllText(path, jsonFile);
        }

    }
}
