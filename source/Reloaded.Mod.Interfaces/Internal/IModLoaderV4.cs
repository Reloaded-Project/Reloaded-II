namespace Reloaded.Mod.Interfaces.Internal;

public interface IModLoaderV4 : IModLoaderV3
{
    /// <summary>
    /// Loads a mod with the given Mod ID.
    /// </summary>
    /// <param name="modId">Unique ID of the mod.</param>
    void LoadMod(string modId);

    /// <summary>
    /// Unloads a mod with the given Mod ID.
    /// </summary>
    /// <param name="modId">Unique ID of the mod.</param>
    void UnloadMod(string modId);

    /// <summary>
    /// Suspends (pauses) a mod with the given Mod ID.
    /// </summary>
    /// <param name="modId">Unique ID of the mod.</param>
    void SuspendMod(string modId);

    /// <summary>
    /// Resumes (unpauses) a mod with the given Mod ID.
    /// </summary>
    /// <param name="modId">Unique ID of the mod.</param>
    void ResumeMod(string modId);

    /// <summary>
    /// Returns a collection containing all currently loaded in mods.
    /// </summary>
    ModInfo[] GetLoadedMods();
}