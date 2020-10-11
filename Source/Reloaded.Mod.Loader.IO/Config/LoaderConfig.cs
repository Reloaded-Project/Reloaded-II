using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reloaded.Mod.Loader.IO.Config.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Config
{
    [Equals(DoNotAddEqualityOperators = true)]
    public class LoaderConfig : ObservableObject
    {
        private const string DefaultApplicationConfigDirectory  = "Apps";
        private const string DefaultModConfigDirectory          = "Mods";
        private const string DefaultPluginConfigDirectory       = "Plugins";
        private const string DefaultLanguageFile                = "en-GB.xaml";
        private const string DefaultThemeFile                   = "Default.xaml";

        private static readonly NugetFeed[] DefaultFeeds        = new NugetFeed[]
        {
            new NugetFeed("Official Repository", "http://167.71.128.50:5000/v3/index.json", "Package repository of Sewer56, the developer of Reloaded. " +
                                                                                            "Contains personal and popular community packages."),
        };

        /// <summary>
        /// Contains the path to the Reloaded Mod Loader II DLL.
        /// </summary>
        [JsonPropertyName("LoaderPath32")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
        public string LoaderPath32 { get; set; } = String.Empty;

        /// <summary>
        /// Contains the path to the Reloaded Mod Loader II DLL.
        /// </summary>
        [JsonPropertyName("LoaderPath64")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
        public string LoaderPath64 { get; set; } = String.Empty;

        /// <summary>
        /// Contains the path to the Reloaded Mod Loader II Launcher.
        /// </summary>
        [JsonPropertyName("LauncherPath")] // Just a safeguard in case someone decides to refactor and native bootstrapper fails to find property.
        public string LauncherPath { get; set; } = String.Empty;

        /// <summary>
        /// Path to the mod loader bootstrapper for X86 processes.
        /// </summary>
        public string Bootstrapper32Path { get; set; } = String.Empty;

        /// <summary>
        /// Path to the mod loader bootstrapper for X64 processes.
        /// </summary>
        public string Bootstrapper64Path { get; set; } = String.Empty;

        /// <summary>
        /// The directory which houses all Reloaded Application information (e.g. Games etc.)
        /// </summary>
        public string ApplicationConfigDirectory { get; set; } = String.Empty;

        /// <summary>
        /// Contains the directory which houses all Reloaded mods.
        /// </summary>
        public string ModConfigDirectory { get; set; } = String.Empty;

        /// <summary>
        /// Contains the directory which houses all Reloaded plugins.
        /// </summary>
        public string PluginConfigDirectory { get; set; } = String.Empty;

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
        /// A list of all available NuGet feeds from which mod packages might be obtained.
        /// </summary>
        public NugetFeed[] NuGetFeeds { get; set; } = DefaultFeeds;

        /* Some mods are universal :wink: */

        public LoaderConfig() { }

        public void SanitizeConfig()
        {
            // System.Text.Json might deserialize this as null.
            EnabledPlugins ??= EmptyArray<string>.Instance;
            NuGetFeeds ??= DefaultFeeds;
            ResetMissingDirectories();
        }

        // Creates directories/folders if they do not exist.
        private void ResetMissingDirectories()
        {
            try
            {
                ApplicationConfigDirectory  = IfNotExistsMakeDefaultDirectory(ApplicationConfigDirectory, DefaultApplicationConfigDirectory);
                ModConfigDirectory          = IfNotExistsMakeDefaultDirectory(ModConfigDirectory, DefaultModConfigDirectory);
                PluginConfigDirectory       = IfNotExistsMakeDefaultDirectory(PluginConfigDirectory, DefaultPluginConfigDirectory);
            }
            catch (Exception)
            {
                /* Access not allowed to directories.*/
            }
        }

        /// <summary>
        /// Returns true if the configuration file exists, else false.
        /// </summary>
        public static bool ConfigurationExists()
        {
            return File.Exists(Paths.LauncherConfigPath);
        }

        /// <summary>
        /// Loads the mod loader configuration from disk.
        /// </summary>
        public static LoaderConfig ReadConfiguration()
        {
            var config = new LoaderConfig();
            if (ConfigurationExists())
            {
                string jsonFile = File.ReadAllText(Paths.LauncherConfigPath);
                config = JsonSerializer.Deserialize<LoaderConfig>(jsonFile);
            }

            config.SanitizeConfig();
            return config;
        }

        /// <summary>
        /// Writes a new mod loader configuration to disk.
        /// </summary>
        /// <param name="config">The new mod loader configuration to write.</param>
        public static void WriteConfiguration(LoaderConfig config)
        {
            string directory = Path.GetDirectoryName(Paths.LauncherConfigPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonFile = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(Paths.LauncherConfigPath, jsonFile);
        }

        // Sets default directory if does not exist.
        private static string IfNotExistsMakeDefaultDirectory(string directoryPath, string defaultDirectory)
        {
            if (!Directory.Exists(directoryPath))
                return CreateDirectoryRelativeToProgram(defaultDirectory);

            return directoryPath;
        }

        /// <summary>
        /// Creates a directory relative to the current assembly directory.
        /// Returns the full path of the supplied directory parameter.
        /// </summary>
        private static string CreateDirectoryRelativeToProgram(string directoryPath)
        {
            string fullDirectoryPath = Path.GetFullPath(Path.Combine(Paths.CurrentProgramFolder, directoryPath));
            Directory.CreateDirectory(fullDirectoryPath);
            return fullDirectoryPath;
        }
    }
}
