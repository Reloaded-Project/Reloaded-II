namespace Reloaded.Mod.Interfaces;

public interface IExports
{
    /// <summary>
    /// The types to be shared with other mods.
    /// i.e. All controller and plugin interfaces.
    /// </summary>
    Type[] GetTypes();

    /// <summary>
    /// An extended interface for providing types to the mod loader.
    /// </summary>
    Type[] GetTypesEx(in ExportsContext context) => Type.EmptyTypes;
}

/// <summary>
/// Provides the context under which IExports will be called. 
/// </summary>
public struct ExportsContext
{
    /// <summary>
    /// The config for the currently associated application.
    /// </summary>
    public IApplicationConfigV1 ApplicationConfig { get; set; }
}