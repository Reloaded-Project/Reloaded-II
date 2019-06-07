using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Interfaces;

namespace Reloaded.Mod.Loader.IO.Utility
{
    /// <summary>
    /// A utility that helps with active cleanup of config files.
    /// </summary>
    public class ConfigCleanupUtility
    {
        /// <summary>
        /// For a given collection of Mod IDs, returns all mod IDs which
        /// have actual corresponding mods.
        /// </summary>
        /// <param name="appIds">List of App IDs.</param>
        public static List<string> FilterNonexistingAppIds(IEnumerable<string> appIds)
        {
            try
            {
                // Get a set of all apps.
                IConfigLoader<ApplicationConfig> configLoader = new ConfigLoader<ApplicationConfig>();
                var allApps = configLoader.ReadConfigurations(
                    new LoaderConfigReader().ReadConfiguration().ApplicationConfigDirectory,
                    ApplicationConfig.ConfigFileName);
                var allAppSet = BuildSet(allApps.Select(tuple => tuple.Object.AppId));

                // Remove nonexisting apps.
                List<string> newAppList = new List<string>(appIds.Count());
                foreach (var appId in appIds)
                {
                    if (allAppSet.Contains(appId))
                        newAppList.Add(appId);
                }

                return newAppList;
            }
            catch (FileNotFoundException ex) // Unit Testing: Config does not exist.
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// For a given collection of Mod IDs, returns all mod IDs which
        /// have actual corresponding mods.
        /// </summary>
        /// <param name="modIds">List of Mod IDs.</param>
        public static List<string> FilterNonexistingModIds(IEnumerable<string> modIds)
        {
            try
            {
                // Get a set of all mods.
                IConfigLoader<ModConfig> configLoader = new ConfigLoader<ModConfig>();
                var allMods = configLoader.ReadConfigurations(new LoaderConfigReader().ReadConfiguration().ModConfigDirectory, ModConfig.ConfigFileName);
                var allModSet = BuildSet(allMods.Select(tuple => tuple.Object.ModId));

                // Remove nonexisting mods.
                List<string> newModList = new List<string>(modIds.Count());
                foreach (var modId in modIds)
                {
                    if (allModSet.Contains(modId))
                        newModList.Add(modId);
                }

                return newModList;
            }
            catch (FileNotFoundException ex) // Unit Testing: Config does not exist.
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Converts an enumerable collection of items into a set.
        /// </summary>
        private static HashSet<T> BuildSet<T>(IEnumerable<T> items)
        {
            HashSet<T> hashmap = new HashSet<T>();

            foreach (var item in items)
                hashmap.Add(item);

            return hashmap;
        }
    }
}
