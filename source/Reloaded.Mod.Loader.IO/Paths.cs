using static System.Environment;

namespace Reloaded.Mod.Loader.IO;

/// <summary>
/// Provides shortcuts to internal Reloaded components and folders.
/// </summary>
public class Paths
{
    private const string BootstrapperDllName = "Reloaded.Mod.Loader.Bootstrapper.dll";

    /// <summary>
    /// Set to true if the currently executing application is the Reloaded launcher.
    /// Affects portable mode behaviour. This has to be manually set by the user.
    /// </summary>
    public static bool IsReloadedLauncher = false;

    /// <summary>
    /// Gets the folder containing this library; and most likely the currently executing program.
    /// </summary>
    public static string CurrentProgramFolder { get; } = Path.GetDirectoryName(typeof(Paths).Assembly.Location);

    /// <summary>
    /// Gets the path of the Reloaded folder in AppData where Reloaded configuration and user data reside.
    /// </summary>
    public static string ConfigFolder { get; } = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "Reloaded-Mod-Loader-II"); // DO NOT CHANGE, C++ BOOTSTRAPPER ALSO DEFINES THIS

    /// <summary>
    /// Location of the static configuration file, used to control the launcher.
    /// </summary>
    public static readonly string LoaderConfigPath = Path.Combine(ConfigFolder, "ReloadedII.json"); // DO NOT CHANGE, C++ BOOTSTRAPPER ALSO DEFINES THIS

    /// <summary>
    /// Location of the logs folder, used to store loader logs.
    /// </summary>
    public static readonly string LogPath = Path.Combine(ConfigFolder, "Logs");

    /// <summary>
    /// Location of the crash dump folder, used to store crash data.
    /// </summary>
    public static readonly string CrashDumpPath = Path.Combine(ConfigFolder, "CrashDumps");

    /// <summary>
    /// Location of the logs archive, used to store old loader logs.
    /// </summary>
    public static readonly string ArchivedLogPath = Path.Combine(LogPath, "OldLogs.zip");

    // MACHINE SPECIFIC DATA //

    /// <summary>
    /// Gets the path of the Reloaded folder in AppData where Reloaded configuration and user data reside.
    /// </summary>
    public static string ConfigFolderLocalPC { get; } = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), "Reloaded-Mod-Loader-II"); // DO NOT CHANGE, C++ BOOTSTRAPPER ALSO DEFINES THIS

    /// <summary>
    /// Location of the cache folder, used to cache external resources.
    /// </summary>
    public static readonly string CachePath = Path.Combine(ConfigFolderLocalPC, "Cache");

    /// <summary>
    /// Location of the cache file, used to cache images.
    /// </summary>
    public static readonly string ImageCachePath = Path.Combine(CachePath, "Image.db");

    /// <summary>
    /// Location of the cache file, used to cache web requests.
    /// </summary>
    public static readonly string WebCachePath = Path.Combine(CachePath, "Web.db");

    /// <summary>
    /// Location of the cache file, used to cache GameBanana mod manifests.
    /// </summary>
    public static readonly string GameBananaManifestCachePath = Path.Combine(CachePath, "GameBananaReleaseManifests.db");

    // UTILITY METHODS //

    /// <summary>
    /// Gets the path of the AnyCPU Reloaded Loader Folder.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetLoaderFolder(string launcherPath) => Path.Combine(launcherPath, $"Loader");

    /// <summary>
    /// Gets the path of the 32-bit Reloaded Loader Folder.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetLoaderFolder32(string launcherPath) => Path.Combine(GetLoaderFolder(launcherPath), "x86");

    /// <summary>
    /// Gets the path of the 64-bit Reloaded Loader Folder.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetLoaderFolder64(string launcherPath) => Path.Combine(GetLoaderFolder(launcherPath), "x64");

    /// <summary>
    /// Gets the path of the 32-bit Reloaded Loader DLL.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetLoaderPath32(string launcherPath) => Path.Combine(GetLoaderFolder32(launcherPath), "Reloaded.Mod.Loader.dll");

    /// <summary>
    /// Gets the path of the 64-bit Reloaded Loader DLL.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetLoaderPath64(string launcherPath) => Path.Combine(GetLoaderFolder64(launcherPath), "Reloaded.Mod.Loader.dll");

    /// <summary>
    /// Gets the path of the 32-bit Reloaded Loader Bootstrapper DLL.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetBootstrapperPath32(string launcherPath) => Path.Combine(GetLoaderFolder32(launcherPath), $"Bootstrapper", Paths.BootstrapperDllName);

    /// <summary>
    /// Gets the path of the 64-bit Reloaded Loader Bootstrapper DLL.
    /// </summary>
    /// <param name="launcherPath">Path to the launcher folder.</param>
    public static string GetBootstrapperPath64(string launcherPath) => Path.Combine(GetLoaderFolder64(launcherPath), $"Bootstrapper", Paths.BootstrapperDllName);
}