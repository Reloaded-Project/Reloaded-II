using System.Windows.Media;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class ImageApplicationPathTuple
    {
        /// <summary>
        /// The image to represent the application by in the GUI.
        /// </summary>
        public ImageSource Image { get; set; }

        /// <summary>
        /// The application configuration.
        /// </summary>
        public IApplicationConfig ApplicationConfig { get; set; }

        /// <summary>
        /// The full path to the application configuration file.
        /// </summary>
        public string ApplicationConfigPath { get; set; }

        public ImageApplicationPathTuple(ImageSource image, IApplicationConfig applicationConfig, string applicationConfigPath)
        {
            Image = image;
            ApplicationConfig = applicationConfig;
            ApplicationConfigPath = applicationConfigPath;
        }

        public override string ToString()
        {
            return ApplicationConfig.AppName;
        }
    }
}
