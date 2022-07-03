namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces;

/// <summary/>
public interface IDependency
{
    /// <summary>
    /// Name assigned to this dependency.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// True if the dependency is available, else false.
    /// </summary>
    bool Available { get; }

    /// <summary>
    /// The operating architecture of the dependency.
    /// </summary>
    Architecture Architecture { get; }

    /// <summary>
    /// Gets the URLs to download the dependency.
    /// </summary>
    Task<string[]> GetUrlsAsync();
}