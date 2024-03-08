namespace Reloaded.Mod.Loader.IO;

/// <summary>
/// Generic class for loading various configuration files such as mod and game configurations.
/// </summary>
/// <typeparam name="TConfigType">
///     A json config class that implements interface <see cref="IConfig"/>.
///     Examples: <see cref="ModConfig"/>, <see cref="ApplicationConfig"/>.
/// </typeparam>
public static class ConfigReader<TConfigType> where TConfigType : IConfig<TConfigType>, new()
{
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
    /// <param name="recurseOnFound">Continues to search in subdirectories even if <see cref="fileName"/> is found.</param>
    /// <returns>Tuples containing the path the configurations was loaded from and the corresponding config class.</returns>
    public static List<PathTuple<TConfigType>> ReadConfigurations(string directory, string fileName, CancellationToken token = default, int maxDepth = 1, int minDepth = 1, bool recurseOnFound = true)
    {
        // Get all config files to load.
        var configurationPaths = IOEx.GetFilesEx(directory, fileName, maxDepth, minDepth, recurseOnFound);

        // Configurations to be returned
        var configurations = new List<PathTuple<TConfigType>>(configurationPaths.Count);
        foreach (string configurationPath in configurationPaths)
        {
            if (token.IsCancellationRequested)
                return configurations;

            if (TryReadConfiguration(configurationPath, out var config))
                configurations.Add(new PathTuple<TConfigType>(configurationPath, config));
        }

        return configurations;
    }

    /* Excluding because path and WriteConfiguration are tested. */

    /// <summary>
    /// Loads a given mod configurations from an absolute file path.
    /// </summary>
    /// <param name="path">The absolute file path of the config file.</param>
    /// <param name="value">The obtained configuration.</param>
    public static bool TryReadConfiguration(string path, out TConfigType value)
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
    public static TConfigType ReadConfiguration(string path) => IConfig<TConfigType>.FromPath(path);

    /// <summary>
    /// Writes a given mod configurations to an absolute file path.
    /// </summary>
    /// <param name="path">The absolute path to write the configurations file to.</param>
    /// <param name="config">The mod configurations to commit to file.</param>
    public static void WriteConfiguration(string path, TConfigType config) => IConfig<TConfigType>.ToPath(config, path);
}