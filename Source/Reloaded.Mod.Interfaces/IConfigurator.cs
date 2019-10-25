namespace Reloaded.Mod.Interfaces
{
    /// <summary>
    /// Provides configuration support for the current mod.
    /// There should only be one configurator in your mod(s).
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Sets the directory where the mod is located.
        /// </summary>
        /// <param name="modDirectory">The full path to the directory where the mod is stored.</param>
        void SetModDirectory(string modDirectory);

        /// <summary>
        /// Returns a list of user configurations.
        /// All configurations are accessed via a PropertyGrid, and as such all configurable items should be public and exposed as properties.
        /// </summary>
        IConfigurable[] GetConfigurations();


        /// <summary>
        /// Allows for custom launcher/configurator implementation.
        /// If you have your own configuration program/code, run that code here and return true.
        /// Else just return false if you want to let Reloaded II's launcher do the configuring.
        /// </summary>
        bool TryRunCustomConfiguration();
    }
}