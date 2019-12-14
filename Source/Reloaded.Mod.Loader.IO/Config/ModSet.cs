using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ModSet : ObservableObject, IConfig
    {
        /* Static Helpers */
        private static readonly ConfigReader<ModSet> _configReader = new ConfigReader<ModSet>();

        /* Class Members */
        public string[] EnabledMods { get; set; }

        public ModSet() { EnabledMods = Constants.EmptyStringArray; }
        public ModSet(IApplicationConfig applicationConfig) => EnabledMods = applicationConfig.EnabledMods;

        /// <summary>
        /// Reads a <see cref="ModSet"/> from the hard disk and returns its contents.
        /// </summary>
        public static ModSet FromFile(string filePath) => _configReader.ReadConfiguration(filePath);

        /// <summary>
        /// Assigns the list of enabled mods to a given application config.
        /// </summary>
        public void ToApplicationConfig(IApplicationConfig config) => config.EnabledMods = EnabledMods;

        /// <summary>
        /// Saves the current mod collection to a given file path.
        /// </summary>
        public void Save(string filePath)
        {
            _configReader.WriteConfiguration(filePath, this);
        }
    }
}
