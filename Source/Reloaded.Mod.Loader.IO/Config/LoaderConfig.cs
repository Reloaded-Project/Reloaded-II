using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class LoaderConfig : ObservableObject
    {
        public const string LoaderDllName = "Reloaded.Mod.Loader.dll";
        public const string Bootstrapper32Name = "Reloaded.Mod.Loader.Bootstrapper.dll";
        public const string Bootstrapper64Name = "Reloaded.Mod.Loader.Bootstrapper.dll";
        public const string Kernel32AddressDumperName = "Kernel32AddressDumper.exe";

        /// <summary>
        /// The name of the configuration file as stored on disk.
        /// </summary>
        public const string ConfigFileName = "ReloadedII.json"; // DO NOT CHANGE, C++ BOOTSTRAPPER ALSO DEFINES THIS

        private const string DefaultApplicationConfigDirectory  = "Apps";
        private const string DefaultModConfigDirectory          = "Mods";
        private const string DefaultPluginConfigDirectory       = "Plugins";

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
        public string[] EnabledPlugins { get; set; }

        public bool FirstLaunch { get; set; } = true;

        /// <summary>
        /// Shows the console window if set to true, else false.
        /// </summary>
        public bool ShowConsole { get; set; } = true;

        /* Some mods are universal :wink: */

        public LoaderConfig()
        {
        }

        // Creates directories/folders if they do not exist.
        public void ResetMissingDirectories()
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

        /* (Mostly) Autogenerated */

        protected bool Equals(LoaderConfig other)
        {
            return string.Equals(LoaderPath32, other.LoaderPath32) &&
                   string.Equals(LoaderPath64, other.LoaderPath64) &&
                   string.Equals(ApplicationConfigDirectory, other.ApplicationConfigDirectory) &&
                   string.Equals(ModConfigDirectory, other.ModConfigDirectory) &&
                   string.Equals(PluginConfigDirectory, other.PluginConfigDirectory) &&
                   EnabledPlugins.SequenceEqualWithNullSupport(other.EnabledPlugins);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != this.GetType())
                return false;

            return Equals((LoaderConfig)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LoaderPath32 != null ? LoaderPath32.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LoaderPath64 != null ? LoaderPath64.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ApplicationConfigDirectory != null ? ApplicationConfigDirectory.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModConfigDirectory != null ? ModConfigDirectory.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PluginConfigDirectory != null ? PluginConfigDirectory.GetHashCode() : 0);

                if (EnabledPlugins != null)
                {
                    foreach (var plugin in EnabledPlugins)
                    {
                        hashCode = (hashCode * 397) ^ plugin.GetHashCode();
                    }
                }

                return hashCode;
            }
        }
    }
}
