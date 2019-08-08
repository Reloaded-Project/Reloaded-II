using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiteNetLib;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Mods;
using Reloaded.Mod.Loader.Server.Messages.Server;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.Mod.Loader.Utilities;
using Reloaded.Mod.Shared;
using Console = Reloaded.Mod.Loader.Logging.Console;

namespace Reloaded.Mod.Loader
{
    public class Loader : IDisposable
    {
        public bool IsLoaded { get; private set; }
        public IApplicationConfig Application { get; private set; }
        public Console Console { get; }
        public PluginManager Manager { get; private set; }
        public LoaderConfig LoaderConfig { get; private set; }

        /// <summary>
        /// This flag suppresses certain exceptions and should only be set to true in unit tests.
        /// </summary>
        public bool IsTesting { get; private set; }

        /// <summary>
        /// Initialize loader in constructor.
        /// </summary>
        public Loader(bool isTesting = false)
        {
            IsTesting = isTesting;
            LoaderConfig = LoaderConfigReader.ReadConfiguration();
            Console = new Console();
            Manager = new PluginManager(this);

            if (LoaderConfig.ShowConsole)
                Console.ShowConsole();
        }

        ~Loader()
        {
            Dispose();
        }

        public void Dispose()
        {
            Manager?.Dispose();
            GC.SuppressFinalize(this);
        }

        /* Public Interface */

        public void LoadMod(string modId)
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);

            // Check for duplicate.
            if (Manager.IsModLoaded(modId))
                throw new ReloadedException(Errors.ModAlreadyLoaded(modId));

            // Note: Code below already ensures no duplicates but it would be nice to
            // throw for the end users of the loader servers so they can see the error.
            var mod      = FindMod(modId, out var allMods);
            var modArray = new[] {(ModConfig) mod.Object};
            LoadModsWithDependencies(modArray, allMods);
        }

        public void UnloadMod(string modId)
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
            Manager.UnloadMod(modId);
        }

        public void SuspendMod(string modId)
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
            Manager.SuspendMod(modId);
        }

        public void ResumeMod(string modId)
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
            Manager.ResumeMod(modId);
        }

        public List<ModInfo> GetLoadedModSummary()
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, true, Errors.ModLoaderNotInitialized);
            return Manager.GetLoadedModSummary();
        }

        /* Methods */

        public void LoadForCurrentProcess()
        {
            var application = FindThisApplication();
            Wrappers.ThrowIfNull(application, Errors.UnableToFindApplication);
            LoadForAppConfig(application);
        }

        /// <summary>
        /// Loads all mods directly into the process.
        /// </summary>
        public void LoadForAppConfig(IApplicationConfig applicationConfig)
        {
            Wrappers.ThrowIfENotEqual(IsLoaded, false, Errors.ModLoaderAlreadyInitialized);
            Application   = applicationConfig;

            // Get all mods and their paths.
            var allModsForApplication  = ApplicationConfig.GetAllMods(Application, out var allMods, LoaderConfig.ModConfigDirectory);

            // Get list of mods to load and load them.
            var modsToLoad = allModsForApplication.Where(x => x.Enabled).Select(x => x.Generic.Object);
            LoadModsWithDependencies(modsToLoad, allMods);
            Manager.LoaderApi.OnModLoaderInitialized();
            IsLoaded = true;
        }

        /// <summary>
        /// Gets a list of all mods from filesystem and returns a mod with a matching ModId.
        /// </summary>
        /// <param name="modId">The modId to find.</param>
        /// <param name="allMods">List of all mod configurations, read during the operation.</param>
        /// <exception cref="ReloadedException">A mod to load has not been found.</exception>
        public PathGenericTuple<IModConfig> FindMod(string modId, out List<PathGenericTuple<ModConfig>> allMods)
        {
            // Get mod with ID
            allMods = ModConfig.GetAllMods(LoaderConfig.ModConfigDirectory);
            var mod = allMods.FirstOrDefault(x => x.Object.ModId == modId);

            if (mod != null)
            {
                var dllPath = ModConfig.GetDllPath(mod.Path, mod.Object);
                return new PathGenericTuple<IModConfig>(dllPath, mod.Object);
            }

            throw new ReloadedException(Errors.ModToLoadNotFound(modId));
        }

        /// <summary>
        /// Loads a collection of mods with their associated dependencies.
        /// </summary>
        private void LoadModsWithDependencies(IEnumerable<ModConfig> modsToLoad, List<PathGenericTuple<ModConfig>> allMods = null)
        {
            // Cache configuration paths for all mods.
            if (allMods == null)
                allMods = ModConfig.GetAllMods(LoaderConfig.ModConfigDirectory);

            var configToPathDictionary = new Dictionary<ModConfig, string>();
            foreach (var mod in allMods)
                configToPathDictionary[mod.Object] = mod.Path;

            // Get dependencies, sort and load in order.
            var dependenciesToLoad  = GetDependenciesForMods(modsToLoad, allMods.Select(x => x.Object), LoaderConfig.ModConfigDirectory);
            var allUniqueModsToLoad = modsToLoad.Concat(dependenciesToLoad).Distinct();
            var allSortedModsToLoad = ModConfig.SortMods(allUniqueModsToLoad);

            var modPaths            = new List<PathGenericTuple<IModConfig>>();
            foreach (var modToLoad in allSortedModsToLoad)
            {
                // Reloaded does not allow loading same mod multiple times.
                if (! Manager.IsModLoaded(modToLoad.ModId))
                {
                    string configPath = configToPathDictionary[modToLoad];
                    string dllPath = ModConfig.GetDllPath(configPath, modToLoad);
                    modPaths.Add(new PathGenericTuple<IModConfig>(dllPath, modToLoad));
                }
            }

            Manager.LoadMods(modPaths);
        }

        /// <summary>
        /// Retrieves all of the dependencies for a given set of mods.
        /// </summary>
        /// <exception cref="FileNotFoundException">A dependency for any of the mods has not been found.</exception>
        private HashSet<ModConfig> GetDependenciesForMods(IEnumerable<ModConfig> mods, IEnumerable<ModConfig> allMods, string modDirectory)
        {
            if (allMods == null)
                allMods = ModConfig.GetAllMods(LoaderConfig.ModConfigDirectory).Select(x => x.Object);

            var dependencies = ModConfig.GetDependencies(mods, allMods, modDirectory);
            if (dependencies.MissingConfigurations.Count > 0 && !IsTesting)
            {
                string missingMods = String.Join(",", dependencies.MissingConfigurations);
                throw new FileNotFoundException($"Reloaded II was unable to find all dependencies for the mod(s) to be loaded.\n" +
                                                $"Aborting load.\n" +
                                                $"Missing dependencies: {missingMods}");
            }

            return dependencies.Configurations;
        }

        /// <summary>
        /// Searches for the application configuration corresponding to the current 
        /// executing application
        /// </summary>
        private IApplicationConfig FindThisApplication()
        {
            var configurations = ApplicationConfig.GetAllApplications(LoaderConfig.ApplicationConfigDirectory);
            var fullPath = Path.GetFullPath(Process.GetCurrentProcess().GetExecutablePath());

            foreach (var configuration in configurations)
            {
                var application = configuration.Object;
                var appLocation = application.AppLocation;

                if (String.IsNullOrEmpty(appLocation))
                    continue;

                var fullAppLocation = Path.GetFullPath(application.AppLocation);
                if (fullAppLocation == fullPath)
                    return application;
            }

            return null;
        }
    }
}
