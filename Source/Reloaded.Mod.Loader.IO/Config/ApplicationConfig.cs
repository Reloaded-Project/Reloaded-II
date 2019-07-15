using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Weaving;
using Rock.Collections;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ApplicationConfig : ObservableObject, Mod.Interfaces.IApplicationConfig, IConfig
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
        public string[] EnabledMods
        {
            get => _enabledMods;
            set => _enabledMods = value ?? Constants.EmptyStringArray;
        }

        private string[] _enabledMods;

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
            _enabledMods = enabledMods;
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
        /// Finds all apps on the filesystem, parses them and returns a list of
        /// all apps.
        /// </summary>
        /// <param name="appDirectory">(Optional) Directory containing all of the applications.</param>
        public static List<PathGenericTuple<ApplicationConfig>> GetAllApplications(string appDirectory = null)
        {
            if (appDirectory == null)
                appDirectory = LoaderConfigReader.ReadConfiguration().ApplicationConfigDirectory;

            return _appConfigReader.ReadConfigurations(appDirectory, ConfigFileName);
        }

        /// <summary>
        /// Returns all mods for this application in load order.
        /// Note: Dependencies are not taken into account in load order - but the mod loader itself does reorder the list taking them into account.
        /// </summary>
        /// <param name="config">The application to get all mods for.</param>
        public static List<BooleanGenericTuple<PathGenericTuple<ModConfig>>> GetAllMods(IApplicationConfig config)
        {
            var modifications = ModConfig.GetAllMods();
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
            var enabledMods = config.EnabledMods;

            // Build set of enabled mods in order of load | O(N^2)
            var enabledModSet = new OrderedHashSet<PathGenericTuple<ModConfig>>(modifications.Count);
            foreach (var enabledMod in enabledMods)
            {
                foreach (var mod in modifications)
                {
                    var modConfig = mod.Object;
                    if (modConfig.ModId == enabledMod)
                    {
                        enabledModSet.Add(mod);

                        break;
                    }
                }
            }

            var totalModList = new List<BooleanGenericTuple<PathGenericTuple<ModConfig>>>(modifications.Count);
            foreach (var mod in enabledModSet)
                totalModList.Add(new BooleanGenericTuple<PathGenericTuple<ModConfig>>(true, mod));

            // Add items not in set.
            foreach (var mod in enabledModSet)
            {
                if (! enabledModSet.Contains(mod))
                    totalModList.Add(new BooleanGenericTuple<PathGenericTuple<ModConfig>>(false, mod));
            }

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
            return _enabledMods.SequenceEqualWithNullSupport(other._enabledMods) && 
                   string.Equals(AppId, other.AppId) && 
                   string.Equals(AppName, other.AppName) && 
                   string.Equals(AppLocation, other.AppLocation) && 
                   string.Equals(AppArguments, other.AppArguments) && 
                   string.Equals(AppIcon, other.AppIcon);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationConfig) obj);
        }

        public override int GetHashCode()
        {
            return (AppId != null ? AppId.GetHashCode() : 0);
        }
    }
}
