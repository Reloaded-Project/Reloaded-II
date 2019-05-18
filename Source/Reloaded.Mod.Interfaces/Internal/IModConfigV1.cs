namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModConfigV1
    {
        /// <summary>
        /// A name that uniquely identifies the mod.
        /// </summary>
        /// <remarks>
        ///     The suggested format to use for names is "game.type.name".
        ///     Example: sonicheroes.asset.seasidehillmidnight     
        /// </remarks>
        string ModId { get; set; }
        
        /// <summary>
        /// Specifies the visible (GUI) name of the mod.
        /// </summary>
        string ModName { get; set; }

        /// <summary>
        /// Specifies the name of the mod author(s).
        /// </summary>
        string ModAuthor { get; set; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        /// <remarks>
        ///     The Reloaded author recommends to use a variant of Semantic Versioning.
        ///     e.g. MAJOR.MINOR.PATCH = 1.1.0
        ///     MAJOR = When a major overhaul of how the modification works at the core is made.
        ///     MINOR = A new feature or functionality is added without changing how mod works at the core.
        ///     PATCH = For when you fix bugs.
        /// </remarks>
        string ModVersion { get; set; }

        /// <summary>
        /// A short description of the modification.
        /// </summary>
        string ModDescription { get; set; }

        /// <summary>
        /// Name of the DLL file associated with the mod.
        /// </summary>
        /// <remarks>
        ///     It is recommended that DLLs are compiled against the AnyCPU architecture preset.
        /// </remarks>
        string ModDll { get; set; }

        /// <summary>
        /// A collection of <see cref="ModId"/>(s) of other modifications that this mod requires to run.
        /// </summary>
        string[] ModDependencies { get; set; }
    }
}
