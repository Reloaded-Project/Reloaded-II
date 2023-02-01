namespace Reloaded.Mod.Launcher.Lib.Interop;

/// <summary>
/// Abstraction for obtaining resources of a generic type from a given source.
/// </summary>
public interface IDictionaryResourceProvider
{
    /// <summary>
    /// Gets a resource value from the resource provider.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>The value of the resource.</returns>
    public TResource Get<TResource>(string resourceName);

    /// <summary>
    /// Sets the value of a resource in the resource provider.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="value">The new value of the resource.</param>
    public void Set<TResource>(string resourceName, TResource value);

    /// <summary>
    /// Gets a resource from the resource provider.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>The value of the resource.</returns>
    public IDictionaryResource<TResource> GetResource<TResource>(string resourceName);
}