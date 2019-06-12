using System.Windows.Media;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class ImageModPathTuple
    {
        private static ConfigReader<ModConfig> _configReader = new ConfigReader<ModConfig>();

        /// <summary>
        /// The URI of the image used to represent the mod by in the GUI.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// The application configuration.
        /// </summary>
        public IModConfig ModConfig { get; set; }

        /// <summary>
        /// The full path to the application configuration file.
        /// </summary>
        public string ModConfigPath { get; set; }

        public ImageModPathTuple(string image, IModConfig modConfig, string modConfigPath)
        {
            Image = image;
            ModConfig = modConfig;
            ModConfigPath = modConfigPath;
        }

        public void Save()
        {
            _configReader.WriteConfiguration(ModConfigPath, (ModConfig)ModConfig);
        }

        public override string ToString()
        {
            return ModConfig.ModName;
        }
    }
}
