using System.Text.Json.Serialization.Metadata;

namespace Reloaded.Mod.Loader.IO.Config;

/// <summary>
/// Interface used to categorize configuration files.
/// </summary>
public interface IConfig<TType> : IConfig where TType : IConfig<TType>, new()
{
    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true
    };
    
    private static Encoding _encoding = new UTF8Encoding(false);

#if NET7_0_OR_GREATER
    /// <summary>
    /// Returns the JSON type info for reflection-less serialization.
    /// </summary>
    public static abstract JsonTypeInfo<TType> GetJsonTypeInfo(out bool supportsSerialize);
#else
    /// <summary>
    /// Returns the JSON type info for reflection-less serialization.
    /// </summary>
    public abstract JsonTypeInfo<TType> GetJsonTypeInfoNet5(out bool supportsSerialize);
#endif

    /// <summary>
    /// Returns the JSON type info for reflection-less serialization.
    /// Wrapper that calls correct underlying method.
    /// </summary>
    public static JsonTypeInfo<TType> GetJsonTypeInfoHelper(out bool supportsSerialize)
    {
#if NET7_0_OR_GREATER
        return TType.GetJsonTypeInfo(out supportsSerialize);
#else
        var instance = Activator.CreateInstance<TType>();
        return instance.GetJsonTypeInfoNet5(out supportsSerialize);
#endif
    }
    
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
        int numAttempts = 0;
        int sleepTime = 32;
        while (true)
        {
            try
            {
                using var stream = IOEx.OpenFile(filePath, FileMode.Open, FileAccess.Read);
                using var textReader = new StreamReader(stream);
                string jsonFile = textReader.ReadToEnd();
                var info = GetJsonTypeInfoHelper(out _);
                var result = info != null ? JsonSerializer.Deserialize(jsonFile, info) 
                    : JsonSerializer.Deserialize<TType>(jsonFile, _options);

                result.SanitizeConfig();
                return result;
            }
            catch (Exception)
            {
                if (numAttempts >= 6)
                    throw;

                numAttempts++;
                Thread.Sleep(sleepTime);
                sleepTime *= 2;
            }
        }
    }

    /// <summary>
    /// Loads a given mod configurations from an absolute file path or default if file does not exist.
    /// </summary>
    /// <param name="filePath">The absolute file path of the config file.</param>
    public static TType FromPathOrDefault(string filePath)
    {
        if (File.Exists(filePath))
            return FromPath(filePath);
            
        var result = new TType();
        result.SanitizeConfig();
        return result;
    }

    /// <summary>
    /// Loads a given mod configurations from an absolute file path.
    /// </summary>
    /// <param name="filePath">The absolute file path of the config file.</param>
    /// <param name="token">Token that can be used to cancel deserialization</param>
    public static async Task<TType> FromPathAsync(string filePath, CancellationToken token = default)
    {
        int numAttempts = 0;
        int sleepTime = 32;

        while (true)
        {
            try
            {
                await using var stream = await IOEx.OpenFileAsync(filePath, FileMode.Open, FileAccess.Read, token);
                var info = GetJsonTypeInfoHelper(out _);
                var result = info != null ? await JsonSerializer.DeserializeAsync(stream, info, token) :
                    await JsonSerializer.DeserializeAsync<TType>(stream, _options, token);
        
                result.SanitizeConfig();
                return result;
            }
            catch (Exception)
            {
                if (numAttempts >= 6)
                    throw;

                numAttempts++;
                await Task.Delay(sleepTime, token);
                sleepTime *= 2;
            }
        }
    }

    /// <summary>
    /// Loads a given mod configurations from an absolute file path or default if file does not exist.
    /// </summary>
    /// <param name="filePath">The absolute file path of the config file.</param>
    /// <param name="token">Token that can be used to cancel deserialization</param>
    public static async Task<TType> FromPathAsyncOrDefault(string filePath, CancellationToken token = default)
    {
        if (File.Exists(filePath))
            return await FromPathAsync(filePath, token);

        var result = new TType();
        result.SanitizeConfig();
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
        CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath));

        var info = GetJsonTypeInfoHelper(out var serialize);
        string jsonFile  = info != null && serialize 
            ? JsonSerializer.Serialize(config, info) 
            : JsonSerializer.Serialize(config, _options);

        var tempPath     = $"{fullPath}.{Path.GetRandomFileName()}";
        
        using (var stream = IOEx.OpenFile(tempPath, FileMode.Create, FileAccess.Write))
        using (var textWriter = new StreamWriter(stream, _encoding))
            textWriter.WriteLine(jsonFile);

        IOEx.MoveFile(tempPath, fullPath);
    }

    /// <summary>
    /// Writes a given mod configurations to an absolute file path.
    /// </summary>
    /// <param name="filePath">The absolute path to write the configurations file to.</param>
    /// <param name="config">The mod configurations to commit to file.</param>
    /// <param name="token">Token that can be used to cancel deserialization</param>
    public static async Task ToPathAsync(TType config, string filePath, CancellationToken token = default)
    {
        string fullPath = Path.GetFullPath(filePath);
        CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath));
        var info = GetJsonTypeInfoHelper(out bool serialize);

        var tempPath = $"{fullPath}.{Path.GetRandomFileName()}";
        try
        {
            await using (var stream = await IOEx.OpenFileAsync(tempPath, FileMode.Create, FileAccess.Write, token))
            {
                if (info != null && serialize)
                    await JsonSerializer.SerializeAsync(stream, config, info, token);
                else
                    await JsonSerializer.SerializeAsync(stream, config, _options, token);
            }

            await IOEx.MoveFileAsync(tempPath, fullPath, token);
        }
        catch (TaskCanceledException)
        {
            File.Delete(tempPath);
        }
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