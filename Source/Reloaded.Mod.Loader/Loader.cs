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
        /// Initialize loader in constructor.
        /// </summary>
        public Loader()
        {
            Console = new Console();
            LoaderConfig = LoaderConfigReader.ReadConfiguration();
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
            Manager.LoadMod(FindMod(modId));
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
            Application = applicationConfig;

            // Get all mods and their paths.
            var allMods = ApplicationConfig.GetAllMods(Application, LoaderConfig.ModConfigDirectory);
            var configToPathDictionary = new Dictionary<ModConfig, string>();

            foreach (var mod in allMods)
                configToPathDictionary[mod.Generic.Object] = mod.Generic.Path;

            // Get list of mods to load and load them.
            var modsToLoad          = allMods.Where(x => x.Enabled).Select(x => x.Generic.Object);
            var dependenciesToLoad  = ModConfig.GetDependencies(modsToLoad, null, LoaderConfig.ModConfigDirectory).Configurations;
            var allUniqueModsToLoad = modsToLoad.Concat(dependenciesToLoad).Distinct();
            var allSortedModsToLoad = ModConfig.SortMods(allUniqueModsToLoad);

            var modPaths            = new List<PathGenericTuple<IModConfig>>();
            foreach (var modToLoad in allSortedModsToLoad)
            {
                string configPath = configToPathDictionary[modToLoad];
                string dllPath = ModConfig.GetDllPath(configPath, modToLoad);
                modPaths.Add(new PathGenericTuple<IModConfig>(dllPath, modToLoad));
            }

            Manager = new PluginManager(this);
            Manager.LoadMods(modPaths);
            Manager.LoaderApi.OnModLoaderInitialized();
            IsLoaded = true;
        }

        /// <summary>
        /// Gets a list of all mods from filesystem and returns a mod with a matching ModId.
        /// </summary>
        /// <param name="modId">The modId to find.</param>
        /// <exception cref="ReloadedException">A mod to load has not been found.</exception>
        public PathGenericTuple<IModConfig> FindMod(string modId)
        {
            // Get mod with ID
            var allMods = ModConfig.GetAllMods(LoaderConfig.ModConfigDirectory);
            var mod = allMods.FirstOrDefault(x => x.Object.ModId == modId);

            if (mod != null)
            {
                var dllPath = ModConfig.GetDllPath(mod.Path, mod.Object);
                return new PathGenericTuple<IModConfig>(dllPath, mod.Object);
            }

            throw new ReloadedException(Errors.ModToLoadNotFound(modId));
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
