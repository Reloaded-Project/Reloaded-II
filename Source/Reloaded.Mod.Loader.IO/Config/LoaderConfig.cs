using System;
using System.IO;
using System.Text.Json.Serialization;
using Reloaded.Mod.Loader.IO.Config.Structs;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Weaving;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class LoaderConfig : ObservableObject
    {
        public const string LoaderDllName = "Reloaded.Mod.Loader.dll";
        public const string Bootstrapper32Name = "Reloaded.Mod.Loader.Bootstrapper.dll";
        public const string Bootstrapper64Name = "Reloaded.Mod.Loader.Bootstrapper.dll";
        public const string Kernel32AddressDumperPath = "Loader/Kernel32AddressDumper.exe";

        /// <summary>
        /// The name of the configuration file as stored on disk.
        /// </summary>
        public const string ConfigFileName = "ReloadedII.json"; // DO NOT CHANGE, C++ BOOTSTRAPPER ALSO DEFINES THIS

        private const string DefaultApplicationConfigDirectory  = "Apps";
        private const string DefaultModConfigDirectory          = "Mods";
        private const string DefaultPluginConfigDirectory       = "Plugins";
        private static readonly NugetFeed[] DefaultFeeds        = new NugetFeed[]
        {
            new NugetFeed("Official Repository", SharedConstants.NuGetApiEndpoint, "Package repository of Sewer56, the developer for Reloaded. " +
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
        public string[] EnabledPlugins { get; set; } = new string[0] { };

        /// <summary>
        /// The language file used by the Reloaded II launcher.
        /// </summary>
        public string LanguageFile { get; set; } = "en-GB.xaml";

        /// <summary>
        /// The theme file used by the Reloaded-II launcher.
        /// </summary>
        public string ThemeFile { get; set; } = "Default.xaml";

        public bool FirstLaunch { get; set; } = true;

        /// <summary>
        /// Shows the console window if set to true, else false.
        /// </summary>
        public bool ShowConsole { get; set; } = true;

        /// <summary>
        /// A list of all available NuGet feeds from which mod packages might be obtained.
        /// </summary>
        public NugetFeed[] NuGetFeeds { get; set; } = DefaultFeeds;

        /* Some mods are universal :wink: */

        public LoaderConfig()
        {
        }

        public void SanitizeConfig()
        {
            // System.Text.Json might deserialize this as null.
            EnabledPlugins ??= Constants.EmptyStringArray;
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

        // Sets default directory if does not exist.
        private static string IfNotExistsMakeDefaultDirectory(string directoryPath, string defaultDirectory)
        {
            if (!Directory.Exists(directoryPath))
                return CreateDirectoryRelativeToCurrent(defaultDirectory);

            return directoryPath;
        }

        /// <summary>
        /// Creates a directory relative to the current directory
        /// and returns the full path of the supplied directory parameter.
        /// </summary>
        private static string CreateDirectoryRelativeToCurrent(string directoryPath)
        {
            string fullDirectoryPath = Path.GetFullPath(directoryPath);
            Directory.CreateDirectory(fullDirectoryPath);
            return fullDirectoryPath;
        }
    }
}
