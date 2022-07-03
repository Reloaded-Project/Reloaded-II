namespace Reloaded.Mod.Launcher.Lib.Interop;

/// <summary>
/// Abstraction for an individual resource from the resource provider. (<see cref="IDictionaryResourceProvider"/>).
/// </summary>
public interface IDictionaryResource<TResource>
{
    /// <summary>
    /// Gets the value of the resource.
    /// </summary>
    public TResource Get();

    /// <summary>
    /// Sets the value of a resource.
    /// </summary>
    /// <param name="value">The value to assign to the resource.</param>
    /// <returns>True if the operation succeeded else false./</returns>
    public bool TrySet(TResource value);
}