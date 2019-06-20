using System.Windows.Media;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class ImageApplicationPathTuple
    {
        private static ConfigReader<ApplicationConfig> _configReader = new ConfigReader<ApplicationConfig>();


        /// <summary>
        /// The image to represent the application by in the GUI.
        /// </summary>
        public ImageSource Image { get; set; }

        /// <summary>
        /// The application configuration.
        /// </summary>
        public ApplicationConfig ApplicationConfig { get; set; }

        /// <summary>
        /// The full path to the application configuration file.
        /// </summary>
        public string ApplicationConfigPath { get; set; }

        public ImageApplicationPathTuple(ImageSource image, ApplicationConfig applicationConfig, string applicationConfigPath)
        {
            Image = image;
            ApplicationConfig = applicationConfig;
            ApplicationConfigPath = applicationConfigPath;
        }

        public void Save()
        {
            _configReader.WriteConfiguration(ApplicationConfigPath, ApplicationConfig);
        }

        public override string ToString()
        {
            return ApplicationConfig.AppName;
        }
    }
}
