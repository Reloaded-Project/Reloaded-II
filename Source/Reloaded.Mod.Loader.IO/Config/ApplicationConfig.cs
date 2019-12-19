using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ApplicationConfig : ObservableObject, IEquatable<ApplicationConfig>, Mod.Interfaces.IApplicationConfig, IConfig
    {
        public const string ConfigFileName = "AppConfig.json";
        
        /* Static defaults to prevent allocating new strings on object creation. */
        private static string DefaultId = "reloaded.application.template";
        private static string DefaultName = "Reloaded Application Template";
        private static string DefaultIcon = "Icon.png";

        private static readonly ConfigReader<ApplicationConfig> _appConfigReader = new ConfigReader<ApplicationConfig>();

        /* Class Members */
        public string AppId                 { get; set; } = DefaultId;
        public string AppName               { get; set; } = DefaultName;
        public string AppLocation           { get; set; } = String.Empty;
        public string AppArguments          { get; set; } = String.Empty;
        public string AppIcon               { get; set; } = DefaultIcon;
        public bool   AutoInject            { get; set; } = false;
        public string[] EnabledMods         { get; set; }

        public ApplicationConfig()
        {

        }

        public ApplicationConfig(string appId, string appName, string appLocation)
        {
            AppId = appId;
            AppName = appName;
            AppLocation = appLocation;
            EnabledMods = Constants.EmptyStringArray;
        }

        public ApplicationConfig(string appId, string appName, string appLocation, string[] enabledMods) : this(appId, appName, appLocation)
        {
            EnabledMods = enabledMods;
        }

        /*
           ---------
           Utilities
           --------- 
        */

        /// <summary>
        /// Writes the configuration to a specified file path.
        /// </summary>
        public static void WriteConfiguration(string path, ApplicationConfig config)
        {
            var applicationConfigLoader = new ConfigReader<ApplicationConfig>();
            applicationConfigLoader.WriteConfiguration(path, config);
        }

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
        public static List<PathGenericTuple<ApplicationConfig>> GetAllApplications(string appDirectory = null, CancellationToken token = default)
        {
            if (appDirectory == null)
                appDirectory = LoaderConfigReader.ReadConfiguration().ApplicationConfigDirectory;

            return _appConfigReader.ReadConfigurations(appDirectory, ConfigFileName, token);
        }

        /// <summary>
        /// Returns all mods for this application in load order. This overload also provides list of all mods.
        /// Note: Dependencies are not taken into account in load order - but the mod loader itself does reorder the list taking them into account.
        /// </summary>
        /// <param name="config">The application to get all mods for.</param>
        /// <param name="allMods">A list of all available modifications, including those not in use by the config.</param>
        /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
        public static List<BooleanGenericTuple<PathGenericTuple<ModConfig>>> GetAllMods(IApplicationConfig config, out List<PathGenericTuple<ModConfig>> allMods, string modDirectory = null)
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
        public static List<BooleanGenericTuple<PathGenericTuple<ModConfig>>> GetAllMods(IApplicationConfig config, string modDirectory = null)
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
        public static List<BooleanGenericTuple<PathGenericTuple<ModConfig>>> GetAllMods(IApplicationConfig config, List<PathGenericTuple<ModConfig>> modifications)
        {
            // Note: Must put items in top to bottom load order.
            var enabledModIds  = config.EnabledMods;

            // Get dictionary of mods by Mod ID
            var modDictionary = new Dictionary<string, PathGenericTuple<ModConfig>>();
            foreach (var mod in modifications)
                modDictionary[mod.Object.ModId] = mod;

            // Add enabled mods.
            var totalModList = new List<BooleanGenericTuple<PathGenericTuple<ModConfig>>>(modifications.Count);
            foreach (var enabledModId in enabledModIds)
            {
                if (modDictionary.ContainsKey(enabledModId))
                    totalModList.Add(new BooleanGenericTuple<PathGenericTuple<ModConfig>>(true, modDictionary[enabledModId]));
            }

            // Add disabled mods.
            var enabledModIdSet = config.EnabledMods.ToHashSet();
            var disabledMods    = modifications.Where(x => !enabledModIdSet.Contains(x.Object.ModId));
            totalModList.AddRange(disabledMods.Select(x => new BooleanGenericTuple<PathGenericTuple<ModConfig>>(false, x)));
            return totalModList;
        }

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

        /*
           ---------------------------------
           Overrides: (Mostly) Autogenerated
           ---------------------------------
        */

        protected bool Equals(ApplicationConfig other)
        {
            return EnabledMods.SequenceEqualWithNullSupport(other.EnabledMods) && 
                   string.Equals(AppId, other.AppId) && 
                   string.Equals(AppName, other.AppName) && 
                   string.Equals(AppLocation, other.AppLocation) && 
                   string.Equals(AppArguments, other.AppArguments) && 
                   string.Equals(AppIcon, other.AppIcon) &&
                   string.Equals(AutoInject, other.AutoInject);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationConfig) obj);
        }

        public override int GetHashCode() => (AppId != null ? AppId.GetHashCode() : 0);

        bool IEquatable<ApplicationConfig>.Equals(ApplicationConfig other)
        {
            return Equals(other);
        }
    }
}
