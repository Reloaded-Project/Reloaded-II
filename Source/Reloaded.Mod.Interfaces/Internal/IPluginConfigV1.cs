namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IPluginConfigV1
    {
        /// <summary>
        /// The name of the Reloaded Mod Loader Plugin.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Short description of the plugin.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The version of the plugin.
        /// </summary>
        /// <remarks>
        ///     The Reloaded author recommends to use a variant of Semantic Versioning.
        ///     e.g. MAJOR.MINOR.PATCH = 1.1.0
        ///     MAJOR = When a major overhaul of how the plugin works at the core is made.
        ///     MINOR = A new feature or functionality is added without changing how plugin works at the core.
        ///     PATCH = For when you fix bugs.
        /// </remarks>
        string Version { get; set; }

        /// <summary>
        /// Specifies the name of the mod author(s).
        /// </summary>
        string Author { get; set; }
    }
}
