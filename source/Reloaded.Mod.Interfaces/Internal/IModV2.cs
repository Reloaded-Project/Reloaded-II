namespace Reloaded.Mod.Interfaces.Internal;

public interface IModV2 : IModV1
{
    /// <summary>
    /// Represents the entry point of the modification.
    /// </summary>
    /// <param name="loader">Interface which allows for the access of Mod Loader specific functionality.</param>
    /// <param name="config">Config for the individual mod.</param>
    void StartEx(IModLoaderV1 loader, IModConfigV1 config) { Start(loader); }
}