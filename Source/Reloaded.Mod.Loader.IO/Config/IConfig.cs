using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Config
{
    /// <summary>
    /// Interface used to categorize configuration files.
    /// </summary>
    public interface IConfig<TType> : IConfig where TType : IConfig<TType>, new()
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
        public static TType FromPath(string filePath)
        {
            using var stream = IOEx.OpenFile(filePath, FileMode.Open, FileAccess.Read);
            using var textReader = new StreamReader(stream);
            string jsonFile = textReader.ReadToEnd();
            var result = JsonSerializer.Deserialize<TType>(jsonFile, _options);
            result.SanitizeConfig();
            return result;
        }

        /// <summary>
        /// Loads a given mod configurations from an absolute file path or default if file does not exist.
        /// </summary>
        /// <param name="filePath">The absolute file path of the config file.</param>
        public static TType FromPathOrDefault(string filePath) => File.Exists(filePath) ? FromPath(filePath) : new TType();

        /// <summary>
        /// Loads a given mod configurations from an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute file path of the config file.</param>
        /// <param name="token">Token that can be used to cancel deserialization</param>
        public static async Task<TType> FromPathAsync(string filePath, CancellationToken token = default)
        {
            await using var stream = await IOEx.OpenFileAsync(filePath, FileMode.Open, FileAccess.Read, token);
            var result = await JsonSerializer.DeserializeAsync<TType>(stream, _options, token);
            result.SanitizeConfig();
            return result;
        }

        /// <summary>
        /// Loads a given mod configurations from an absolute file path or default if file does not exist.
        /// </summary>
        /// <param name="filePath">The absolute file path of the config file.</param>
        /// <param name="token">Token that can be used to cancel deserialization</param>
        public static async Task<TType> FromPathAsyncOrDefault(string filePath, CancellationToken token = default) => File.Exists(filePath) ? await FromPathAsync(filePath, token) : new TType();

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute path to write the configurations file to.</param>
        /// <param name="config">The mod configurations to commit to file.</param>
        public static void ToPath(TType config, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath));

            string jsonFile = JsonSerializer.Serialize(config, _options);
            using var stream = IOEx.OpenFile(fullPath, FileMode.Create, FileAccess.Write);
            using var textWriter = new StreamWriter(stream, Encoding.UTF8);
            textWriter.WriteLine(jsonFile);
        }

        /// <summary>
        /// Writes a given mod configurations to an absolute file path.
        /// </summary>
        /// <param name="filePath">The absolute path to write the configurations file to.</param>
        /// <param name="config">The mod configurations to commit to file.</param>
        /// <param name="token">Token that can be used to cancel deserialization</param>
        public static async void ToPathAsync(TType config, string filePath, CancellationToken token = default)
        {
            string fullPath = Path.GetFullPath(filePath);
            CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath));

            await using var stream = await IOEx.OpenFileAsync(fullPath, FileMode.Create, FileAccess.Write, token);
            await JsonSerializer.SerializeAsync(stream, config, _options, token);
        }

        private static void CreateDirectoryIfNotExist(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Base interface for configurations.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Makes fixes to deserialized config where appropriate.
        /// </summary>
        void SanitizeConfig() { }
    }
}
