using Reloaded.Mod.Interfaces;
using TestModControlParams.Configuration;

namespace TestModControlParams.Template.Configuration
{
    /// <summary>
    /// Creates the various different configurations used by the mod.
    /// These configurations are available in the dropdown in Reloaded launcher. 
    /// </summary>
    public class ConfiguratorMixinBase
    {
        /// <summary>
        /// Defines the configuration items to create.
        /// </summary>
        /// <param name="configFolder">Folder storing the configuration.</param>
        public virtual IUpdatableConfigurable[] MakeConfigurations(string configFolder)
        {
            // You can add any Configurable here.
            return new IUpdatableConfigurable[]
            {
            Configurable<Config>.FromFile(Path.Combine(configFolder, "Config.json"), "Default Config")
            };
        }

        /// <summary>
        /// Allows for custom launcher/configurator implementation.
        /// If you have your own configuration program/code, run that code here and return true, else return false.
        /// </summary>
        public virtual bool TryRunCustomConfiguration(Configurator configurator)
        {
            return false;
        }

        #region Config Migration (Must implement if coming from old mod template with config in mod folder)
        /// <summary>
        /// Migrates from the old config location (usually mod folder) to the newer config location (separate folder).
        /// </summary>
        /// <param name="oldDirectory">Old directory containing the mod configs.</param>
        /// <param name="newDirectory">New directory pointing to user config folder.</param>
        public virtual void Migrate(string oldDirectory, string newDirectory)
        {
            // Uncomment to move files from older to newer config directory.
            // TryMoveFile("Config.json");

#pragma warning disable CS8321
            void TryMoveFile(string fileName)
            {
                try { File.Move(Path.Combine(oldDirectory, fileName), Path.Combine(newDirectory, fileName)); }
                catch (Exception) { /* Ignored */ }
            }
#pragma warning restore CS8321
        }
        #endregion
    }
}