using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// Class responsible for maintaining the cleanliness of various configurations such as mods/apps/loader.
    /// </summary>
    public class ConfigCleaner
    {
        private List<PathGenericTuple<ApplicationConfig>> _applicationConfigs;
        private List<PathGenericTuple<ModConfig>> _modConfigs;
        private LoaderConfig _loaderConfig;

        private HashSet<string> _allAppsSet;
        private HashSet<string> _allModsSet;

        private readonly ConfigReader<ModConfig> _modConfigReader = new ConfigReader<ModConfig>();
        private readonly ConfigReader<ApplicationConfig> _appConfigReader = new ConfigReader<ApplicationConfig>();

        /// <summary>
        /// Constructs a configuration cleaner which reads the default <see cref="LoaderConfig"/> from the filesystem.
        /// </summary>
        public ConfigCleaner()
        {
            LoaderConfig loaderConfig;
            try { loaderConfig = LoaderConfigReader.ReadConfiguration();  }
            catch (Exception) { loaderConfig = new LoaderConfig(); }
            SetupConfigCleaner(loaderConfig);
        }

        /// <summary>
        /// Constructs a configuration cleaner which uses a supplied <see cref="LoaderConfig"/>.
        /// </summary>
        public ConfigCleaner(LoaderConfig loaderConfig)
        {
            SetupConfigCleaner(loaderConfig);
        }

        private void SetupConfigCleaner(LoaderConfig loaderConfig)
        {
            _loaderConfig       = loaderConfig;
            _modConfigs         = ModConfig.GetAllMods(_loaderConfig.ModConfigDirectory);
            _applicationConfigs = ApplicationConfig.GetAllApplications(_loaderConfig.ApplicationConfigDirectory);

            _allAppsSet = BuildSet(_applicationConfigs.Select(tuple => tuple.Object.AppId));
            _allModsSet = BuildSet(_modConfigs.Select(tuple => tuple.Object.ModId));
        }

        public void Clean()
        {
            CleanupLoaderConfig();
            CleanupApplicationConfigs();
            CleanupModConfigs();
        }

        /// <summary>
        /// Public only for testing. Please use <see cref="Clean"/>
        /// </summary>
        public bool CleanupModConfig(PathGenericTuple<ModConfig> tuple)
        {
            var conf = tuple.Object;
            bool saveNeeded = false;

            if (String.IsNullOrEmpty(conf.ModName))
            {
                conf.ModName = "Unknown Reloaded Mod";
                saveNeeded = true;
            }

            if (String.IsNullOrEmpty(conf.ModId))
            {
                conf.ModId = conf.ModName.Replace(" ", ".");
                saveNeeded = true;
            }

            if (!String.IsNullOrEmpty(conf.ModIcon))
            {
                string imagePath = Path.Combine(Path.GetDirectoryName(tuple.Path) ?? "", conf.ModIcon);
                if (!File.Exists(imagePath))
                {
                    conf.ModIcon = "";
                    saveNeeded = true;
                }
            }

            var oldModDependencies = conf.ModDependencies;
            var newModDependencies = FilterNonexistingModIds(conf.ModDependencies).ToArray();
            if (oldModDependencies.Length != newModDependencies.Length)
            {
                conf.ModDependencies = newModDependencies;
                saveNeeded = true;
            }

            var oldAppIds = conf.SupportedAppId;
            var newAppIds = FilterNonexistingAppIds(conf.SupportedAppId).ToArray();
            if (oldAppIds.Length != newAppIds.Length)
            {
                conf.SupportedAppId = newAppIds;
                saveNeeded = true;
            }

            return saveNeeded;
        }

        /// <summary>
        /// Public only for testing. Please use <see cref="Clean"/>
        /// </summary>
        public bool CleanupApplicationConfig(PathGenericTuple<ApplicationConfig> tuple)
        {
            bool saveNeeded = false;
            var conf = tuple.Object;

            if (String.IsNullOrEmpty(conf.AppName))
            {
                conf.AppName = "Reloaded Application Name";
                saveNeeded = true;
            }

            if (String.IsNullOrEmpty(conf.AppId))
            {
                conf.AppId = conf.AppName.Replace(" ", ".");
                saveNeeded = true;
            }

            if (!String.IsNullOrEmpty(conf.AppIcon))
            {
                string imagePath = Path.Combine(Path.GetDirectoryName(tuple.Path) ?? "", conf.AppIcon);
                if (!File.Exists(imagePath))
                {
                    conf.AppIcon = "";
                    saveNeeded = true;
                }
            }

            // Remove nonexisting images.
            if (tuple.Path != "")
            {
                string[] potentialImages = Directory.GetFiles(Path.GetDirectoryName(tuple.Path) ?? "", "*");
                potentialImages = potentialImages.Where(x => Constants.WpfSupportedFormatsArray.Any(x.EndsWith)).ToArray();

                foreach (var image in potentialImages)
                {
                    if (Path.GetFileName(image) != conf.AppIcon)
                        File.Delete(image);
                }
            }

            if (!File.Exists(conf.AppLocation))
            {
                conf.AppLocation = "";
                conf.AppArguments = "";
                saveNeeded = true;
            }

            var oldEnabledMods = conf.EnabledMods;
            var newEnabledMods = FilterNonexistingModIds(conf.EnabledMods).ToArray();
            if (oldEnabledMods.Length != newEnabledMods.Length)
            {
                conf.EnabledMods = newEnabledMods;
                saveNeeded = true;
            }

            // Write back new config.
            return saveNeeded;
        }

        /// <summary>
        /// Public only for testing. Please use <see cref="Clean"/>
        /// </summary>
        public LoaderConfig CleanupLoaderConfig(LoaderConfig config)
        {
            // Reset any directories.
            config.ResetMissingDirectories();

            return config;
        }

        /* Utility Methods */

        private List<string> FilterNonexistingAppIds(IEnumerable<string> appIds)
        {
            List<string> newAppList = new List<string>();
            foreach (var appId in appIds)
            {
                if (_allAppsSet.Contains(appId))
                    newAppList.Add(appId);
            }

            return newAppList;
        }

        private List<string> FilterNonexistingModIds(IEnumerable<string> modIds)
        {
            List<string> newModList = new List<string>();
            foreach (var modId in modIds)
            {
                if (_allModsSet.Contains(modId))
                    newModList.Add(modId);
            }

            return newModList;
        }

        private void CleanupLoaderConfig()
        {
            CleanupLoaderConfig(_loaderConfig);
            LoaderConfigReader.WriteConfiguration(_loaderConfig);
        }

        private void CleanupModConfigs()
        {
            Parallel.ForEach(_modConfigs, (tuple) =>
            {
                bool needsSaving = CleanupModConfig(tuple);
                if (needsSaving)
                    _modConfigReader.WriteConfiguration(tuple.Path, tuple.Object);
            });
        }

        private void CleanupApplicationConfigs()
        {
            Parallel.ForEach(_applicationConfigs, tuple =>
            {
                bool needsSaving = CleanupApplicationConfig(tuple);
                if (needsSaving)
                    _appConfigReader.WriteConfiguration(tuple.Path, tuple.Object);
            });
        }

        /// <summary>
        /// Converts an enumerable collection of items into a set.
        /// </summary>
        private HashSet<T> BuildSet<T>(IEnumerable<T> items)
        {
            HashSet<T> hashmap = new HashSet<T>();

            foreach (var item in items)
                hashmap.Add(item);

            return hashmap;
        }
    }
}
