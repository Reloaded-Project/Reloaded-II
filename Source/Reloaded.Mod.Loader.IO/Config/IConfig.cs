using System.IO;
using System.Text.Json;

namespace Reloaded.Mod.Loader.IO.Config
{
    /// <summary>
    /// Interface used to categorize configuration files.
    /// </summary>
    public interface IConfig<TType> : IConfig where TType : IConfig<TType>
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions() { WriteIndented = true };

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute path to write the configurations file to.</param>
        public void ToPath(string filePath) => ToPath((TType) this, filePath);

        /// <summary>
        /// Loads a given mod configurations from an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute file path of the config file.</param>
        static TType FromPath(string filePath)
        {
            string jsonFile = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<TType>(jsonFile, _options);
            result.SetNullValues();
            return result;
        }

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute path to write the configurations file to.</param>
        /// <param name="config">The mod configurations to commit to file.</param>
        public static void ToPath(TType config, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string directoryOfPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonSerializer.Serialize(config, _options);
            File.WriteAllText(fullPath, jsonFile);
        }
    }

    /// <summary>
    /// Base interface for configurations.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Sets null values to default where appropriate.
        /// </summary>
        void SetNullValues() { }
    }
}
