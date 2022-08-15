namespace Reloaded.Mod.Interfaces.Internal;

public interface IModLoaderV3 : IModLoaderV2
{
    /// <summary>
    /// Gets the directory storing the mod user configuration for a mod
    /// with a specified id.
    /// </summary>
    /// <param name="modId">The id of an individual mod.</param>
    public string GetModConfigDirectory(string modId);
}