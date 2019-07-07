using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Reloaded.Messaging.Structs;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Mods;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server.Messages.Server;
using Reloaded.Mod.Loader.Utilities;
using Console = Reloaded.Mod.Loader.Logging.Console;

namespace Reloaded.Mod.Loader
{
    public class Loader
    {
        public IApplicationConfig ThisApplication { get; private set; }
        public Console Console { get; private set; }
        public PluginManager Manager { get; private set; }

        /// <summary>
        /// Initialize loader in constructor.
        /// </summary>
        public Loader()
        {
            ThisApplication = FindThisApplication();
            Wrappers.ThrowIfNull(ThisApplication, Constants.Errors.UnableToFindApplication);

            Console = new Console();
            LoadMods();

            Manager.LoaderApi.OnModLoaderInitialized();
        }

        /* Public Interface */

        public void GetLoadedMods(ref NetMessage<GetLoadedMods> message)
        {
            Manager.GetLoadedMods(ref message);
        }

        public void LoadMod(string modId)
        {
            Manager.LoadMod(FindMod(modId));
        }

        public void UnloadMod(string modId)
        {
            Manager.UnloadMod(modId);
        }

        public void SuspendMod(string modId)
        {
            Manager.SuspendMod(modId);
        }

        public void ResumeMod(string modId)
        {
            Manager.ResumeMod(modId);
        }

        /* Methods */

        /// <summary>
        /// Gets a list of all mods from filesystem and returns a mod with a matching ModId
        /// </summary>
        /// <param name="modId">The modId to find.</param>
        /// <exception cref="ReloadedException">A mod to load has not been found.</exception>
        private PathGenericTuple<IModConfig> FindMod(string modId)
        {
            // Get mod with ID
            var allMods = ApplicationConfig.GetAllMods(ThisApplication);
            var mod = allMods.FirstOrDefault(x => x.Generic.Object.ModId == modId);

            if (mod != null)
            {
                var modPathTuple = mod.Generic;
                return new PathGenericTuple<IModConfig>(modPathTuple.Path, modPathTuple.Object);
            }

            throw new ReloadedException("Mod to load has not been found");
        }

        /// <summary>
        /// Searches for the application configuration corresponding to the current 
        /// executing application
        /// </summary>
        private IApplicationConfig FindThisApplication()
        {
            var configurations = ApplicationConfig.GetAllApplications();
            var fullPath = Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName);

            foreach (var configuration in configurations)
            {
                var application = configuration.Object;
                var appLocation = Path.GetFullPath(application.AppLocation);
                if (appLocation == fullPath)
                {
                    return application;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads all mods directly into the process.
        /// </summary>
        private void LoadMods()
        {
            // Get all mods and their paths.
            var allMods = ApplicationConfig.GetAllMods(ThisApplication);
            var configToPathDictionary = new Dictionary<ModConfig, string>();

            foreach (var mod in allMods)
                configToPathDictionary[mod.Generic.Object] = mod.Generic.Path;

            // Get list of mods to load and load them.
            var modsToLoad = allMods.Where(x => x.Enabled).Select(x => x.Generic.Object);
            var sortedModsToLoad = ModConfig.SortMods(modsToLoad);
            var modPaths = new List<PathGenericTuple<IModConfig>>();

            foreach (var modToLoad in sortedModsToLoad)
            {
                string configPath = configToPathDictionary[modToLoad];
                string dllPath = ModConfig.GetDllPath(configPath, modToLoad);
                modPaths.Add(new PathGenericTuple<IModConfig>(dllPath, modToLoad));
            }

            Manager = new PluginManager(modPaths, this);
        }
    }
}
