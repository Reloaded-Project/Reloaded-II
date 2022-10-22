namespace Reloaded.Mod.Loader.IO.Config;

[DebuggerDisplay("{ModId}")]
[Equals(DoNotAddEqualityOperators = true)]
public class ModUserConfig : ObservableObject, IConfig<ModUserConfig>, IModUserConfig
{
    public const string ConfigFileName = "ModUserConfig.json";

    /* Class Members */
    public string ModId { get; set; }

    public Dictionary<string, object> PluginData { get; set; }

    public bool IsUniversalMod { get; set; }

    public bool? AllowPrereleases { get; set; }

    /*
       ---------
       Utilities
       --------- 
    */

    /// <summary>
    /// Finds all mod user configs on the filesystem, parses them and returns a list of all mod user configs.
    /// </summary>
    /// <param name="configDirectory">(Optional) Directory containing all of the applications.</param>
    /// <param name="token">Optional token used to cancel the operation.</param>
    public static List<PathTuple<ModUserConfig>> GetAllUserConfigs(string configDirectory = null, CancellationToken token = default)
    {
        if (configDirectory == null)
            configDirectory = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath).GetApplicationConfigDirectory();

        return ConfigReader<ModUserConfig>.ReadConfigurations(configDirectory, ConfigFileName, token, 2);
    }

    /// <summary>
    /// Retrieves an user config folder for a given mod.
    /// </summary>
    /// <param name="modId">Id for the mod to get the user config for.</param>
    /// <param name="configDirectory">The directory containing the user configurations.</param>
    public static string GetUserConfigFolderForMod(string modId, string configDirectory = null)
    {
        if (configDirectory == null)
            configDirectory = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath).GetModUserConfigDirectory();
            
        return Path.Combine(configDirectory, IOEx.ForceValidFilePath(modId));
    }

    /// <summary>
    /// Retrieves the main file storing user configuration for a mod.
    /// </summary>
    /// <param name="modId">Id for the mod to get the user config for.</param>
    /// <param name="configDirectory">The directory containing the user configurations.</param>
    public static string GetUserConfigPathForMod(string modId, string configDirectory = null) => Path.Combine(GetUserConfigFolderForMod(modId, configDirectory), ConfigFileName);

    // Reflection-less JSON
    public static JsonTypeInfo<ModUserConfig> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = true;
        return ModUserConfigContext.Default.ModUserConfig;
    }
    
    public JsonTypeInfo<ModUserConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
}