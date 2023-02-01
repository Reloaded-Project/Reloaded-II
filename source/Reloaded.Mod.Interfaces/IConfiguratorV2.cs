namespace Reloaded.Mod.Interfaces;

/// <summary>
/// Version 2 of the configurator.
/// With support for migration from Version 1 and custom config directory.
/// </summary>
public interface IConfiguratorV2 : IConfiguratorV1
{
    /// <summary>
    /// Migrates the config files location from an older to a newer directory.
    /// </summary>
    /// <param name="oldDirectory">The location of the old mod config directory, usually the mod folder if migrating from V1.</param>
    /// <param name="newDirectory">The location of the new mod config directory, usually a new directory.</param>
    void Migrate(string oldDirectory, string newDirectory);

    /// <summary>
    /// Sets the directory where the config is located.
    /// </summary>
    /// <param name="configDirectory">The full path to where the config directory is stored.</param>
    void SetConfigDirectory(string configDirectory);
}