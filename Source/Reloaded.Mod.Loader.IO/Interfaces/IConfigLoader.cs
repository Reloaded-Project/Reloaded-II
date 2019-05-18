using System.Collections.Generic;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO.Interfaces
{
    /// <summary>
    /// Contains a generic interface for loading various configurations files
    /// such as mod and game configurations.
    /// </summary>
    /// <typeparam name="TConfigType">
    ///     A json config class that implements interface <see cref="IConfig"/>.
    ///     Examples: <see cref="ModConfig"/> and <see cref="ApplicationConfig"/>.
    /// </typeparam>
    public interface IConfigLoader<TConfigType> where TConfigType : IConfig
    {
        /// <summary>
        /// Recursively searches all directories under the provided absolute directory paths
        /// and loads all configurations with a matching file name.
        /// </summary>
        /// <param name="directory">The absolute path of the directory from which to load all configurations from.</param>
        /// <param name="fileName">The name of the file to load. The filename can contain wildcards * but not regex.</param>
        /// <returns>Tuples containing the path the configurations was loaded from and the corresponding config class.</returns>
        List<PathGenericTuple<TConfigType>> ReadConfigurations(string directory, string fileName);

        /// <summary>
        /// Writes all file path and configurations tuples returned from <see cref="ReadConfigurations(string, string)"/> back to disk.
        /// </summary>
        /// <param name="configurations">List of file path and config class tuples to write to disk.</param>
        void WriteConfigurations(PathGenericTuple<TConfigType>[] configurations);

        /// <summary>
        /// Loads a given mod configurations from an absolute file path.
        /// </summary>
        /// <param name="path">The absolute file path of the config file.</param>
        TConfigType ReadConfiguration(string path);

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="path">The absolute path to write the configurations file to.</param>
        /// <param name="modConfig">The mod configurations to commit to file.</param>
        void WriteConfiguration(string path, TConfigType modConfig);
    }
}
