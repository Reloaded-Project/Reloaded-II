namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Interface used to append additional metadata to mod configurations.
/// </summary>
public interface IDependencyMetadataWriter
{
    /// <summary>
    /// Writes necessary metadata to a given mod configuration.
    /// </summary>
    /// <param name="mod">The mod for which the metadata write should be performed.</param>
    /// <param name="dependencies">Collection of all dependencies for the mod.</param>
    /// <returns>True if the config was updated, else false.</returns>
    bool Update(ModConfig mod, IEnumerable<ModConfig> dependencies);
}