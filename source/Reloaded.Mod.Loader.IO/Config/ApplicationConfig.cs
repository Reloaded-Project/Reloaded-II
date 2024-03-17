using Reloaded.Mod.Loader.IO.Config.Contexts;

namespace Reloaded.Mod.Loader.IO.Config;

[Equals(DoNotAddEqualityOperators = true, DoNotAddGetHashCode = true)]
public class ApplicationConfig : ObservableObject, IApplicationConfig, IConfig<ApplicationConfig>
{
    public const string ConfigFileName = "AppConfig.json";
        
    /* Static defaults to prevent allocating new strings on object creation. */
    private static string DefaultId = "reloaded.application.template";
    private static string DefaultName = "Reloaded Application Template";
    private static string DefaultIcon = "Icon.png";

    /* Class Members */
    public string AppId                 { get; set; } = DefaultId;
    public string AppName               { get; set; } = DefaultName;
    public string AppLocation           { get; set; } = String.Empty;
    public string AppArguments          { get; set; } = String.Empty;
    public string AppIcon               { get; set; } = DefaultIcon;
    public bool   AutoInject            { get; set; } = false;
    public string[] EnabledMods         { get; set; }
    public string WorkingDirectory      { get; set; }

    // V2

    /// <summary>
    /// Data stored by plugins. Maps a unique string key to arbitrary data.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; } = new Dictionary<string, object>();

    // V3 (Launcher only right now)
    public string[] SortedMods { get; set; }
    public bool PreserveDisabledModOrder { get; set; } = true; // <= default to 'true' for new configs, and 'false' for configs on older versions.
    public bool DontInject { get; set; } = false; // don't inject loader, start game via regular process launch.
    public bool IsMsStore { get; set; } = false; // attempts to unprotect this binary on every launch.

    /*
       --------------
       Create/Destroy
       --------------
    */

    public ApplicationConfig() { }

    public ApplicationConfig(string appId, string appName, string appLocation, string workingDirectory = null)
    {
        AppId = appId;
        AppName = appName;
        AppLocation = appLocation;
        EnabledMods = EmptyArray<string>.Instance;
        SortedMods = EmptyArray<string>.Instance;
        WorkingDirectory = workingDirectory;
    }

    public ApplicationConfig(string appId, string appName, string appLocation, string[] enabledMods, string workingDirectory = null) : this(appId, appName, appLocation, workingDirectory)
    {
        EnabledMods = enabledMods;
    }

    /// <summary>
    /// Attempts to obtain the location of the application icon.
    /// </summary>
    /// <param name="configPath">Path to the application configuration.</param>
    /// <param name="logoFilePath">The file path to the logo. This returns a valid file path, even if the actual logo file does not exist.</param>
    /// <returns>True if the logo file exists, else false.</returns>
    public bool TryGetApplicationIcon(string configPath, out string logoFilePath) => TryGetApplicationIcon(configPath, this, out logoFilePath);

    /*
       ---------
       Utilities
       --------- 
    */

    /// <summary>
    /// Attempts to obtain the location of the application icon.
    /// </summary>
    /// <param name="configPath">Path to the application configuration.</param>
    /// <param name="config">The application configuration itself.</param>
    /// <param name="logoFilePath">The file path to the logo. This returns a valid file path, even if the actual logo file does not exist.</param>
    /// <returns>True if the logo file exists, else false.</returns>
    public static bool TryGetApplicationIcon(string configPath, ApplicationConfig config, out string logoFilePath)
    {
        // Set default icon path if emptied.
        if (String.IsNullOrEmpty(config.AppIcon))
            config.AppIcon = DefaultIcon;

        // Try find icon.
        string logoDirectory = Path.GetDirectoryName(configPath);
        logoFilePath = Path.Combine(logoDirectory, config.AppIcon);

        if (File.Exists(logoFilePath))
            return true;

        return false;
    }

    /// <summary>
    /// Finds all apps on the filesystem, parses them and returns a list of
    /// all apps.
    /// </summary>
    /// <param name="appDirectory">(Optional) Directory containing all of the applications.</param>
    /// <param name="token">Optional token used to cancel the operation.</param>
    public static List<PathTuple<ApplicationConfig>> GetAllApplications(string appDirectory = null, CancellationToken token = default)
    {
        if (appDirectory == null)
            appDirectory = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath).GetApplicationConfigDirectory();

        return ConfigReader<ApplicationConfig>.ReadConfigurations(appDirectory, ConfigFileName, token, 2);
    }

    /// <summary>
    /// Returns all mods for this application in load order. This overload also provides list of all mods.
    /// Note: Dependencies are not taken into account in load order - but the mod loader itself does reorder the list taking them into account.
    /// </summary>
    /// <param name="config">The application to get all mods for.</param>
    /// <param name="allMods">A list of all available modifications, including those not in use by the config.</param>
    /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
    public static List<BooleanGenericTuple<PathTuple<ModConfig>>> GetAllMods(IApplicationConfig config, out List<PathTuple<ModConfig>> allMods, string modDirectory = null)
    {
        allMods = ModConfig.GetAllMods(modDirectory);
        return GetAllMods(config, allMods);
    }

    /// <summary>
    /// Returns all mods for this application in load order.
    /// Note: Dependencies are not taken into account in load order - but the mod loader itself does reorder the list taking them into account.
    /// </summary>
    /// <param name="config">The application to get all mods for.</param>
    /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
    public static List<BooleanGenericTuple<PathTuple<ModConfig>>> GetAllMods(IApplicationConfig config, string modDirectory = null)
    {
        var modifications = ModConfig.GetAllMods(modDirectory);
        return GetAllMods(config, modifications);
    }

    /// <summary>
    /// Returns all mods for this application in load order.
    /// Note: Dependencies are not taken into account in load order - but the mod loader itself does reorder the list taking them into account.
    /// </summary>
    /// <param name="config">The application to get all mods for.</param>
    /// <param name="modifications">List of modifications to retrieve all mods from.</param>
    public static List<BooleanGenericTuple<PathTuple<ModConfig>>> GetAllMods(IApplicationConfig config, List<PathTuple<ModConfig>> modifications)
    {
        // Note: Must put items in top to bottom load order.
        var enabledModIds  = config.EnabledMods;

        // Get dictionary of mods by Mod ID
        var modDictionary = new Dictionary<string, PathTuple<ModConfig>>();
        foreach (var mod in modifications)
            modDictionary[mod.Config.ModId] = mod;

        // Add enabled mods.
        var totalModList = new List<BooleanGenericTuple<PathTuple<ModConfig>>>(modifications.Count);
        foreach (var enabledModId in enabledModIds)
        {
            if (modDictionary.ContainsKey(enabledModId))
                totalModList.Add(new BooleanGenericTuple<PathTuple<ModConfig>>(true, modDictionary[enabledModId]));
        }

        // Add disabled mods.
        var enabledModIdSet = config.EnabledMods.ToHashSet();
        var disabledMods    = modifications.Where(x => !enabledModIdSet.Contains(x.Config.ModId));
        totalModList.AddRange(disabledMods.Select(x => new BooleanGenericTuple<PathTuple<ModConfig>>(false, x)));
        return totalModList;
    }

    /// <summary>
    /// Converts the relative or absolute application location to a full path.
    /// </summary>
    /// <returns>The full path to the app location.</returns>
    public static string GetAbsoluteAppLocation(PathTuple<ApplicationConfig> config)
    {
        var location = config.Config.AppLocation;
        var basePath = Path.GetDirectoryName(config.Path)!;
        string finalPath;

        // Specific for windows paths starting on \ - they need the drive added to them.
        // I constructed this piece like this for possible Mono support.
        if (!Path.IsPathRooted(location) || "\\".Equals(Path.GetPathRoot(location)))
        {
            if (location.StartsWith(Path.DirectorySeparatorChar))
                finalPath = Path.Combine(Path.GetPathRoot(basePath)!, location.TrimStart(Path.DirectorySeparatorChar));
            else
                finalPath = Path.Combine(basePath, location);
        }
        else
        {
            finalPath = location;
        }

        // Resolves any internal "..\" to get the true full path.
        return Path.GetFullPath(finalPath);
    }

    /// <summary>
    /// Replaces the current Application ID with a known alias.
    /// </summary>
    public static string AliasAppId(string input)
    {
        return input switch
        {
            "p4pc_dt_mc.exe" => "p4g.exe", // Persona 4 Golden 64-bit (MS Store)
            _ => input
        };
    }

    // Reflection-less JSON
    public static JsonTypeInfo<ApplicationConfig> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = false;
        return ApplicationConfigContext.Default.ApplicationConfig;
    }
    
    public JsonTypeInfo<ApplicationConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);

    /*
        ---------
        Overrides
        ---------
    */

    /* Useful for debugging. */
    public override string ToString()
    {
        return $"AppName: {AppName}, AppLocation: {AppLocation}";
    }

    /// <inheritdoc />
    public void SanitizeConfig()
    {
        EnabledMods ??= EmptyArray<string>.Instance;
        SortedMods ??= EmptyArray<string>.Instance;
    }

    public override int GetHashCode() => (AppId != null ? AppId.GetHashCode() : 0);
}