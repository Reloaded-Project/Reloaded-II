namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces
{
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
        /// Gets the URLs to download the dependency.
        /// </summary>
        string[] GetUrls();
    }
}