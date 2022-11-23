using static System.String;

namespace Reloaded.Mod.Loader.IO.Config;

[Equals(DoNotAddEqualityOperators = true)]
public class LoaderConfig : ObservableObject, IConfig<LoaderConfig>
{
    private const string DefaultApplicationConfigDirectory  = "Apps";
    private const string DefaultModConfigDirectory          = "Mods";
    private const string DefaultMiscConfigDirectory         = "User/Misc";
    private const string DefaultModUserConfigDirectory      = "User/Mods";
    private const string DefaultPluginConfigDirectory       = "Plugins";
    private const string DefaultLanguageFile                = "en-GB.xaml";
    private const string DefaultThemeFile                   = "Default.xaml";
    private const string PortableModeFile                   = "portable.txt";

    private static readonly NugetFeed[] DefaultFeeds        = new NugetFeed[]
    {
        new NugetFeed("Official Repository", "http://packages.sewer56.moe:5000/v3/index.json", "Package repository of Sewer56, the developer of Reloaded. " +
            "Contains personal and popular community packages."),
    };

    /// <summary>
    /// Contains the path to the Reloaded Mod Loader II DLL.
    /// </summary>
    [JsonPropertyName("LoaderPath32")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
    public string LoaderPath32 { get; set; } = Empty;

    /// <summary>
    /// Contains the path to the Reloaded Mod Loader II DLL.
    /// </summary>
    [JsonPropertyName("LoaderPath64")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
    public string LoaderPath64 { get; set; } = Empty;

    /// <summary>
    /// Contains the path to the Reloaded Mod Loader II Launcher.
    /// </summary>
    [JsonPropertyName("LauncherPath")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
    public string LauncherPath { get; set; } = Empty;

    /// <summary>
    /// Path to the mod loader bootstrapper for X86 processes.
    /// </summary>
    public string Bootstrapper32Path { get; set; } = Empty;

    /// <summary>
    /// Path to the mod loader bootstrapper for X64 processes.
    /// </summary>
    public string Bootstrapper64Path { get; set; } = Empty;

    /// <summary>
    /// The directory which houses all Reloaded Application information (e.g. Games etc.)
    /// </summary>
    [JsonInclude]
    public string ApplicationConfigDirectory { internal get; set; } = Empty;

    /// <summary>
    /// Contains the directory which houses all Reloaded mod user configurations.
    /// </summary>
    [JsonInclude]
    public string ModUserConfigDirectory { internal get; set; } = Empty;

    /// <summary>
    /// Contains the directory which houses all miscellaneous Reloaded configurations.
    /// </summary>
    [JsonInclude]
    public string MiscConfigDirectory { internal get; set; } = Empty;

    /// <summary>
    /// Contains the directory which houses all Reloaded plugins.
    /// </summary>
    [JsonInclude]
    public string PluginConfigDirectory { internal get; set; } = Empty;

    /// <summary>
    /// Contains the directory which houses all Reloaded mods.
    /// </summary>
    [JsonInclude]
    public string ModConfigDirectory { internal get; set; } = Empty;

    /// <summary>
    /// Contains a list of all plugins that are enabled, by config paths relative to plugin directory.
    /// </summary>
    public string[] EnabledPlugins { get; set; } = EmptyArray<string>.Instance;

    /// <summary>
    /// The language file used by the Reloaded II launcher.
    /// </summary>
    public string LanguageFile { get; set; } = DefaultLanguageFile;

    /// <summary>
    /// The theme file used by the Reloaded-II launcher.
    /// </summary>
    public string ThemeFile { get; set; } = DefaultThemeFile;

    public bool FirstLaunch { get; set; } = true;

    /// <summary>
    /// Shows the console window if set to true, else false.
    /// </summary>
    public bool ShowConsole { get; set; } = true;

    /// <summary>
    /// Amount of time in hours since last modified that log files get updated.
    /// </summary>
    public int LogFileCompressTimeHours { get; set; } = 6;

    /// <summary>
    /// Amount of time in hours since last modified that log files get deleted.
    /// </summary>
    public int LogFileDeleteHours { get; set; } = 336;

    /// <summary>
    /// Amount after which leftover crash dumps are deleted.
    /// </summary>
    public int CrashDumpDeleteHours { get; set; } = 24;

    /// <summary>
    /// A list of all available NuGet feeds from which mod packages might be obtained.
    /// </summary>
    public NugetFeed[] NuGetFeeds { get; set; } = DefaultFeeds;

    /// <summary>
    /// Force early versions of mods to be downloaded.
    /// </summary>
    public bool ForceModPrereleases { get; set; } = false;

    /// <summary>
    /// Time interval between launcher scanning which processes for a specific application have Reloaded running in them.
    /// </summary>
    public int ReloadedProcessListRefreshInterval { get; set; } = 1000;

    /// <summary>
    /// The amount of time given to the mod loader to setup all of its mods by the application launcher.
    /// </summary>
    public int LoaderSetupTimeout { get; set; } = 30000;

    /// <summary>
    /// Time between successive connection attempts to the remote mod loader.
    /// </summary>
    public int LoaderSetupSleeptime { get; set; } = 32;

    /// <summary>
    /// Time interval between launcher scanning which processes for a specific application have Reloaded running in them.
    /// </summary>
    public int ProcessRefreshInterval { get; set; } = 200;

    /// <summary>
    /// True if the loader is in `Portable Mode`.
    /// In Portable mode, the locations of <see cref="ApplicationConfigDirectory"/>, <see cref="ModConfigDirectory"/>, <see cref="ModUserConfigDirectory"/> and <see cref="PluginConfigDirectory"/>
    /// are ignored and replaced with default, `Apps`, `Mods` and `Plugins` folders.
    /// </summary>
    [JsonIgnore]
    public bool UsePortableMode { get; set; }

    /// <summary>
    /// Skips launch warnings related to WINE.
    /// </summary>
    public bool SkipWineLaunchWarning { get; set; }
    
    /// <summary>
    /// Disables DirectInput support, used for people with input device issues.
    /// </summary>
    public bool DisableDInput { get; set; }

    private string _launcherFolder;

    /* Some mods are universal :wink: */

    public LoaderConfig() { }

    public void SanitizeConfig()
    {
        // System.Text.Json might deserialize this as null.
        EnabledPlugins ??= EmptyArray<string>.Instance;
        NuGetFeeds ??= DefaultFeeds;
        ResetMissingDirectories();
        CleanEmptyFeeds();
        RerouteDefaultFeed();
    }

    public string GetModUserConfigDirectory() => UsePortableMode ? GetDirectoryRelativeToLauncherFolder(DefaultModUserConfigDirectory) : ModUserConfigDirectory;

    public string GetPluginConfigDirectory() => UsePortableMode ? GetDirectoryRelativeToLauncherFolder(DefaultPluginConfigDirectory) : PluginConfigDirectory;

    public string GetApplicationConfigDirectory() => UsePortableMode ? GetDirectoryRelativeToLauncherFolder(DefaultApplicationConfigDirectory) : ApplicationConfigDirectory;

    public string GetModConfigDirectory() => UsePortableMode ? GetDirectoryRelativeToLauncherFolder(DefaultModConfigDirectory) : ModConfigDirectory;

    public string GetMiscConfigDirectory() => UsePortableMode ? GetDirectoryRelativeToLauncherFolder(DefaultMiscConfigDirectory) : MiscConfigDirectory;

    private void RerouteDefaultFeed()
    {
        foreach (var feed in NuGetFeeds)
        {
            if (feed.URL.Equals("http://167.71.128.50:5000/v3/index.json", StringComparison.OrdinalIgnoreCase))
                feed.URL = DefaultFeeds[0].URL;
        }
    }

    // Creates directories/folders if they do not exist.
    private void ResetMissingDirectories()
    {
        try
        {
            UsePortableMode = IsInPortableMode();
            if (UsePortableMode)
            {
                // Create default directories (if needed).
                Directory.CreateDirectory(GetApplicationConfigDirectory());
                Directory.CreateDirectory(GetModConfigDirectory());
                Directory.CreateDirectory(GetModUserConfigDirectory());
                Directory.CreateDirectory(GetPluginConfigDirectory());
                Directory.CreateDirectory(GetMiscConfigDirectory());
            }

            ApplicationConfigDirectory  = SetDefaultDirectory(ApplicationConfigDirectory, DefaultApplicationConfigDirectory);
            MiscConfigDirectory         = SetDefaultDirectory(MiscConfigDirectory, DefaultMiscConfigDirectory);
            ModUserConfigDirectory      = SetDefaultDirectory(ModUserConfigDirectory, DefaultModUserConfigDirectory);
            ModConfigDirectory          = SetDefaultDirectory(ModConfigDirectory, DefaultModConfigDirectory);
            PluginConfigDirectory       = SetDefaultDirectory(PluginConfigDirectory, DefaultPluginConfigDirectory);
        }
        catch (Exception)
        {
            /* Access not allowed to directories.*/
        }
    }

    /// <summary>
    /// Updates the paths referenced in this config file.
    /// </summary>
    /// <param name="launcherDirectory">The directory in which the launcher is contained.</param>
    /// <param name="dllNotFoundText">Error text to display when a DLL is not found.</param>
    /// <exception cref="DllNotFoundException">A required DLL is not found.</exception>
    public void UpdatePaths(string launcherDirectory, string dllNotFoundText = "")
    {
        if (String.IsNullOrEmpty(launcherDirectory))
            throw new DllNotFoundException("The provided launcher directory is null or empty. This is a bug. Report this to the developer.");

        // Loader configuration.
        var loaderPath32 = Paths.GetLoaderPath32(launcherDirectory);
        if (!File.Exists(loaderPath32))
            throw new DllNotFoundException($"(x86) {Path.GetFileName(loaderPath32)} {dllNotFoundText}");

        var loaderPath64 = Paths.GetLoaderPath64(launcherDirectory);
        if (!File.Exists(loaderPath64))
            throw new DllNotFoundException($"(x64) {Path.GetFileName(loaderPath64)} {dllNotFoundText}");

        // Bootstrappers.
        var bootstrapper32Path = Paths.GetBootstrapperPath32(launcherDirectory);
        if (!File.Exists(bootstrapper32Path))
            throw new DllNotFoundException($"{Path.GetFileName(bootstrapper32Path)} {dllNotFoundText}");

        var bootstrapper64Path = Paths.GetBootstrapperPath64(launcherDirectory);
        if (!File.Exists(bootstrapper64Path))
            throw new DllNotFoundException($"{Path.GetFileName(bootstrapper64Path)} {dllNotFoundText}");

        // Set to config.
        LauncherPath = Process.GetCurrentProcess().MainModule!.FileName;
        LoaderPath32 = loaderPath32;
        LoaderPath64 = loaderPath64;
        Bootstrapper32Path = bootstrapper32Path;
        Bootstrapper64Path = bootstrapper64Path;
    }

    // Removes empty NuGet feeds.*
    private void CleanEmptyFeeds()
    {
        NuGetFeeds = NuGetFeeds?.Where(x => !IsNullOrEmpty(x.URL)).ToArray();
    }

    // Sets default directory if does not exist.
    private string SetDefaultDirectory(string directoryPath, string defaultDirectory)
    {
        if (!Directory.Exists(directoryPath))
            return CreateDirectoryRelativeToLauncherFolder(defaultDirectory);

        return directoryPath;
    }

    /// <summary>
    /// Gets a directory relative to the path of the program.
    /// </summary>
    private string GetDirectoryRelativeToLauncherFolder(string directoryPath)
    {
        return Path.GetFullPath(Path.Combine(GetLauncherFolder(), directoryPath));
    }

    /// <summary>
    /// Creates a directory relative to the current assembly directory.
    /// Returns the full path of the supplied directory parameter.
    /// </summary>
    private string CreateDirectoryRelativeToLauncherFolder(string directoryPath)
    {
        string fullDirectoryPath = GetDirectoryRelativeToLauncherFolder(directoryPath);
        Directory.CreateDirectory(fullDirectoryPath);
        return fullDirectoryPath;
    }

    /// <summary>
    /// Returns true if portable mode (always use launcher folder) is enabled.
    /// </summary>
    private bool IsInPortableMode()
    {
        var currentFolderPath = Path.GetFullPath(Path.Combine(GetLauncherFolder(), PortableModeFile));
        return File.Exists(currentFolderPath);
    }

    private string GetLauncherFolder()
    {
        if (_launcherFolder != null)
            return _launcherFolder;

        // Use override if launcher flag is true.
        if (Paths.IsReloadedLauncher)
            return NormalizePath(Paths.CurrentProgramFolder);

        // Get launcher path.
        var launcherPath = LauncherPath;
        if (string.IsNullOrEmpty(LauncherPath))
            launcherPath = Path.Combine(Paths.CurrentProgramFolder, "Dummy.txt");

        // Workaround for when non-launcher folder is used.
        // e.g. When using loader.
        var launcherFolder = NormalizePath(Paths.CurrentProgramFolder);
        var launcherFolderFallback = NormalizePath(Path.GetDirectoryName(launcherPath));
        if (!launcherFolder.Equals(launcherFolderFallback, StringComparison.OrdinalIgnoreCase))
            launcherFolder = launcherFolderFallback;

        _launcherFolder = launcherFolder;
        return launcherFolder;

        static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    // Reflection-less JSON
    public static JsonTypeInfo<LoaderConfig> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = false;
        return LoaderConfigContext.Default.LoaderConfig;
    }
    
    public JsonTypeInfo<LoaderConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
}