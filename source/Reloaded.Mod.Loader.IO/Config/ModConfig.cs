using Reloaded.Memory.Extensions;

namespace Reloaded.Mod.Loader.IO.Config;

[Equals(DoNotAddEqualityOperators = true, DoNotAddGetHashCode = true)]
public class ModConfig : ObservableObject, IConfig<ModConfig>, IModConfig
{
    /* Constants */
    public const string ConfigFileName  = "ModConfig.json";
    public const string IconFileName    = "Preview.png";

    /* Static defaults to prevent allocating new strings on object creation. */
    private const string DefaultId = "reloaded.template.modconfig";
    private const string DefaultName = "Reloaded Mod Config Template";
    private const string DefaultAuthor = "Someone";
    private const string DefaultVersion = "1.0.0";
    private const string DefaultDescription = "Template for a Reloaded Mod Configuration";

    /* Class members. */
    public string ModId             { get; set; } = DefaultId;
    public string ModName           { get; set; } = DefaultName;
    public string ModAuthor         { get; set; } = DefaultAuthor;
    public string ModVersion        { get; set; } = DefaultVersion;
    public string ModDescription    { get; set; } = DefaultDescription;
    public string ModDll            { get; set; } = String.Empty;
    
    [DoNotCheckEquality]
    public string ModIcon           { get; set; } = String.Empty;
    public string ModR2RManagedDll32 { get; set; } = String.Empty;
    public string ModR2RManagedDll64 { get; set; } = String.Empty;
    public string ModNativeDll32    { get; set; } = String.Empty;
    public string ModNativeDll64    { get; set; } = String.Empty;
    public string[] Tags            { get; set; } = Array.Empty<string>();
    public bool?  CanUnload         { get; set; } = null;
    public bool?  HasExports        { get; set; } = null;
    public bool   IsLibrary         { get; set; } = false;
    public string ReleaseMetadataFileName { get; set; } = "Sewer56.Update.ReleaseMetadata.json";

    [JsonIgnore]
    public string ModSubDirs { get; set; } = string.Empty;

    [JsonIgnore]
    public string ModDisplayName => ModSubDirs.Length <= 0 ? ModName : $"{ModSubDirs}/{ModName}";

    /// <summary>
    /// Data stored by plugins. Maps a unique string key to arbitrary data.
    /// </summary>
    public Dictionary<string, object> PluginData { get; set; } = new Dictionary<string, object>();

    public bool IsUniversalMod { get; set; } = false;

    public string[] ModDependencies         { get; set; } = Array.Empty<string>();
    public string[] OptionalDependencies    { get; set; } = Array.Empty<string>();
    public string[] SupportedAppId          { get; set; } = Array.Empty<string>();
    public string ProjectUrl { get; set; } = String.Empty;

    /*
       ---------------
       Class Functions
       ---------------
    */

    /// <summary>
    /// Returns true if the mod is native, else false.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    public bool IsNativeMod(string configPath)
    {
        return IsNativeMod(configPath, this);
    }

    /// <summary>
    /// Returns true if the mod consists of ReadyToRun (R2R) executables, else false.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    public bool IsR2R(string configPath)
    {
        return IsR2R(configPath, this);
    }

    /// <summary>
    /// Retrieves the path to the individual managed DLL for this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    public string GetManagedDllPath(string configPath)
    {
        return GetManagedDllPath(configPath, this);
    }

    /// <summary>
    /// Retrieves the path to the individual DLL (managed or native) for this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    public string GetDllPath(string configPath)
    {
        return IsNativeMod(configPath) ? GetNativeDllPath(configPath) : GetManagedDllPath(configPath);
    }

    /// <summary>
    /// Retrieves the path to the native DLL for this mod, autodetecting if 32 or 64 bit..
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    public string GetNativeDllPath(string configPath)
    {
        return Environment.Is64BitProcess ? GetNativeDllPath64(configPath, this) : GetNativeDllPath32(configPath, this);
    }

    /// <summary>
    /// Tries to retrieve the full path to the icon that represents this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="iconPath">Full path to the icon.</param>
    public bool TryGetIconPath(string configPath, out string iconPath)
    {
        return TryGetIconPath(configPath, this, out iconPath);
    }

    /// <summary>
    /// Checks if any DLL paths are set for this config.
    /// </summary>
    public bool HasDllPath()
    {
        return HasDllPath(this);
    }

    /// <summary>
    /// Refresh the sub directories of the mod, and by extension also the Mod Display Name.
    /// </summary>
    public void RefreshSubdirectoryPaths(string configDirectory, string modPath)
    {
        var relativePath = Path.GetRelativePath(configDirectory, modPath);
        unsafe
        {
            fixed (char* ptr = relativePath)
            {
                var chars = new Span<char>(ptr, relativePath.Length);
                chars.Replace('\\', '/', chars);
                
                // Find the index for the start of the second-to-last directory to be removed
                var lastSlashIndex = chars.LastIndexOf('/');
                if (lastSlashIndex > 0)
                    lastSlashIndex  = chars.SliceFast(0, lastSlashIndex - 1).LastIndexOf('/');

                ModSubDirs = lastSlashIndex > 0 
                    ? chars.SliceFast(0, lastSlashIndex).ToString() 
                    : ""; // no folder prefix
            }
        }
    }

    /*
       ---------
       Utilities
       --------- 
    */

    /// <summary>
    /// Retrieves the path to the individual DLL for this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetManagedDllPath(string configPath, ModConfig modConfig)
    {
        if (IsR2R(configPath, modConfig))
            return Environment.Is64BitProcess ? GetR2RManagedDllPath64(configPath, modConfig) : GetR2RManagedDllPath32(configPath, modConfig);
            
        string configDirectory = Path.GetDirectoryName(configPath);
        return Path.Combine(configDirectory, modConfig.ModDll);
    }

    /// <summary>
    /// Retrieves the path to the individual DLL for this mod if the mod is using 32-bit Ready2Run format.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetR2RManagedDllPath32(string configPath, ModConfig modConfig)
    {
        string configDirectory = Path.GetDirectoryName(configPath);
        return Path.Combine(configDirectory, modConfig.ModR2RManagedDll32);
    }

    /// <summary>
    /// Retrieves the path to the individual DLL for this mod if the mod is using 64-bit Ready2Run format.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetR2RManagedDllPath64(string configPath, ModConfig modConfig)
    {
        string configDirectory = Path.GetDirectoryName(configPath);
        return Path.Combine(configDirectory, modConfig.ModR2RManagedDll64);
    }

    /// <summary>
    /// Returns true if the mod is native, else false.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static bool IsNativeMod(string configPath, ModConfig modConfig)
    {
        return !String.IsNullOrEmpty(modConfig.ModNativeDll64) || !String.IsNullOrEmpty(modConfig.ModNativeDll32);
    }

    /// <summary>
    /// Returns true if the mod consists of ReadyToRun (R2R) executables, else false.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The mod configuration in question.</param>
    public static bool IsR2R(string configPath, ModConfig modConfig)
    {
        return (!String.IsNullOrEmpty(modConfig.ModR2RManagedDll32) && File.Exists(GetR2RManagedDllPath32(configPath, modConfig))) ||
               (!String.IsNullOrEmpty(modConfig.ModR2RManagedDll64) && File.Exists(GetR2RManagedDllPath64(configPath, modConfig)));
    }

    /// <summary>
    /// Retrieves the path to the native 32-bit DLL for this mod, autodetecting if 32 or 64 bit..
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetNativeDllPath(string configPath, ModConfig modConfig)
    {
        return Environment.Is64BitProcess ? GetNativeDllPath64(configPath, modConfig) : GetNativeDllPath32(configPath, modConfig);
    }

    /// <summary>
    /// Retrieves the path to native 32-bit DLL for this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetNativeDllPath32(string configPath, ModConfig modConfig)
    {
        string configDirectory = Path.GetDirectoryName(configPath);
        return Path.Combine(configDirectory, modConfig.ModNativeDll32);
    }

    /// <summary>
    /// Retrieves the path to native 64-bit DLL for this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static string GetNativeDllPath64(string configPath, ModConfig modConfig)
    {
        string configDirectory = Path.GetDirectoryName(configPath);
        return Path.Combine(configDirectory, modConfig.ModNativeDll64);
    }

    /// <summary>
    /// Checks if any DLL paths are set for this config.
    /// </summary>
    /// <param name="modConfig">The individual mod configuration.</param>
    public static bool HasDllPath(ModConfig modConfig)
    {
        return !string.IsNullOrEmpty(modConfig.ModDll)
               || !string.IsNullOrEmpty(modConfig.ModNativeDll32) || !string.IsNullOrEmpty(modConfig.ModNativeDll64)
               || !string.IsNullOrEmpty(modConfig.ModR2RManagedDll32) || !string.IsNullOrEmpty(modConfig.ModR2RManagedDll64);
    }

    /// <summary>
    /// Tries to retrieve the full path to the icon that represents this mod.
    /// </summary>
    /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathTuple{TGeneric}"/>)</param>
    /// <param name="modConfig">The individual mod configuration.</param>
    /// <param name="iconPath">Full path to the icon.</param>
    public static bool TryGetIconPath(string configPath, ModConfig modConfig, out string iconPath)
    {
        if (String.IsNullOrEmpty(configPath))
        {
            iconPath = null;
            return false;
        }

        iconPath = Path.Combine(Path.GetDirectoryName(configPath), modConfig.ModIcon);
        return File.Exists(iconPath);
    }

    /// <summary>
    /// Finds all mods on the filesystem, parses them and returns a list of all mods.
    /// </summary>
    /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
    /// <param name="token">Allows for premature cancellation of the task.</param>
    public static List<PathTuple<ModConfig>> GetAllMods(string modDirectory = null, CancellationToken token = default)
    {
        if (modDirectory == null)
            modDirectory = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath).GetModConfigDirectory();

        var modConfigs =
            ConfigReader<ModConfig>.ReadConfigurations(modDirectory, ConfigFileName, token, int.MaxValue, 2, false);
        var existingConfigs = new HashSet<string>();

        // Remove duplicate mods. 
        // Hopefully user doesn't have any, but if they do, this will delete them.
        for (var x = modConfigs.Count - 1; x >= 0; x--)
        {
            var alreadyExists = existingConfigs.Add(modConfigs[x].Config.ModId);
            if (!alreadyExists)
                modConfigs.RemoveAt(x);
        }
        
        foreach (var cfg in modConfigs)
        {
            cfg.Config.RefreshSubdirectoryPaths(modDirectory, cfg.Path);
        }

        return modConfigs;
    }

    /// <summary>
    /// Returns a list of all of present and missing dependencies for a set of configurations.
    /// Note: Does not include optional dependencies.
    /// </summary>
    /// <param name="configurations">The mod configurations.</param>
    /// <param name="allMods">(Optional) List of all available mods.</param>
    /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static ModDependencySet GetDependencies(IEnumerable<ModConfig> configurations, IEnumerable<ModConfig> allMods = null, string modDirectory = null)
    {
        var allModsCollection   = allMods ?? GetAllMods(modDirectory).Select(x => x.Config);
        var dependencySets      = new List<ModDependencySet>();

        foreach (var config in configurations)
            dependencySets.Add(GetDependencies(config, allModsCollection));
            
        return new ModDependencySet(dependencySets);
    }

    /// <summary>
    /// Returns a list of all of present and missing dependencies for a given configuration.
    /// Note: Does not include optional dependencies.
    /// </summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="allMods">(Optional) List of all available mods.</param>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static ModDependencySet GetDependencies(ModConfig config, IEnumerable<ModConfig> allMods = null)
    {
        if (allMods == null)
            allMods = GetAllMods().Select(x => x.Config);

        var dependencySet = new ModDependencySet();

        // Populate whole list of nodes (graph).
        var allModsDict = PopulateNodeDictionary(allMods);

        // Recursive resolution.
        GetDependenciesVisitNode(new Node<ModConfig>(config), allModsDict, dependencySet);

        return dependencySet;
    }

    /// <summary>
    /// Sorts a list of mods, taking into account individual dependencies between mods.
    /// Note: Does not include optional dependencies.
    /// </summary>
    public static List<ModConfig> SortMods(IEnumerable<ModConfig> mods)
    {
        var sortedMods = new List<ModConfig>();

        // Populate whole list of nodes (graph).
        var allModsDict = PopulateNodeDictionary(mods);

        // Generate list of edges to other nodes this node is dependent on.
        var allMods = allModsDict.Values.ToArray();
        foreach (var node in allMods)
            PopulateNodeDependencies(node, allModsDict, null);

        // Perform a depth first search so long a node is unvisited.
        Node<ModConfig> firstUnvisitedNode;
        while ((firstUnvisitedNode = allMods.FirstOrDefault(node => node.Visited == Mark.NotVisited)) != null)
        {
            SortModsVisitNode(firstUnvisitedNode, sortedMods);
        }

        return sortedMods;
    }

    /// <summary>
    /// Recursive function which given an unsorted node and the list it's going to be stored in, arranges
    /// a set of mod configurations.
    /// </summary>
    private static void SortModsVisitNode(Node<ModConfig> node, List<ModConfig> sortedMods)
    {
        // Already fully visited, already in list.
        if (node.Visited == Mark.Visited)
            return;

        // Disallow looping on itself.
        // Not a directed acyclic graph.
        if (node.Visited == Mark.Visiting)
            return;

        node.Visited = Mark.Visiting;

        // Visit all children, depth first.
        foreach (var dependency in node.Edges)
            SortModsVisitNode(dependency, sortedMods);

        node.Visited = Mark.Visited;
        sortedMods.Add(node.Element);
    }

    /// <summary>
    /// Returns a list of all Mod IDs that are not installed referenced by a mod config.
    /// </summary>
    /// <param name="node">The mod configuration for which to find missing dependencies.</param>
    /// <param name="allModsDict">Collection containing all of the mod configurations.</param>
    /// <param name="dependencySet">Accumulator for all missing dependencies.</param>
    private static void GetDependenciesVisitNode(Node<ModConfig> node, Dictionary<string, Node<ModConfig>> allModsDict, ModDependencySet dependencySet)
    {
        // Already fully visited, already in list.
        if (node.Visited == Mark.Visited)
            return;

        // Disallow looping on itself.
        // Not a directed acyclic graph.
        if (node.Visited == Mark.Visiting)
            return;

        node.Visited = Mark.Visiting;

        // Get all dependencies (children/edge nodes).
        PopulateNodeDependencies(node, allModsDict, dependencySet);

        // Visit all children, depth first.
        foreach (var dependency in node.Edges)
            GetDependenciesVisitNode(dependency, allModsDict, dependencySet);

        // Set visited and return to next in stack.
        node.Visited = Mark.Visited;
    }

    private static void PopulateNodeDependencies(Node<ModConfig> node, Dictionary<string, Node<ModConfig>> allModsDict, ModDependencySet dependencySet)
    {
        // Populates the dependencies for each node given a dictionary of all available mods.
        node.Edges = new List<Node<ModConfig>>(allModsDict.Count);
        foreach (string dependencyId in node.Element.ModDependencies)
        {
            if (allModsDict.TryGetValue(dependencyId, out var dependency))
            {
                node.Edges.Add(dependency);
                dependencySet?.Configurations.Add(dependency.Element);
            }
            else
            {
                dependencySet?.MissingConfigurations.Add(dependencyId);
            }
        }
    }

    private static Dictionary<string, Node<ModConfig>> PopulateNodeDictionary(IEnumerable<ModConfig> mods)
    {
        var allModsDict = new Dictionary<string, Node<ModConfig>>(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in mods)
            allModsDict[mod.ModId] = new Node<ModConfig>(mod);

        return allModsDict;
    }

    /*
       ---------
       Overrides
       ---------
    */

    /* Useful for debugging. */
    public override string ToString()
    {
        return $"ModId: {ModId}, ModName: {ModName}";
    }

    /// <inheritdoc />
    public void SanitizeConfig()
    {
        ModDependencies ??= EmptyArray<string>.Instance;
        OptionalDependencies ??= EmptyArray<string>.Instance;
        SupportedAppId ??= EmptyArray<string>.Instance;
    }

    // Reflection-less JSON
    public static JsonTypeInfo<ModConfig> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = false;   
        return ModConfigContext.Default.ModConfig;
    }
    
    public JsonTypeInfo<ModConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);

    /*
       ---------
       Overrides
       ---------
    */

    public override int GetHashCode() => (ModId != null ? ModId.GetHashCode() : 0);
}