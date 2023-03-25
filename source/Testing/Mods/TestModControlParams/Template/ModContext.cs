using Reloaded.Mod.Interfaces;
using TestModControlParams.Configuration;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace TestModControlParams.Template
{
    /// <summary>
    /// Represents information passed in from the mod loader template to the implementing mod.
    /// </summary>
    public class ModContext
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        public IModLoader ModLoader { get; set; } = null!;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        public IReloadedHooks? Hooks { get; set; } = null!;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        public ILogger Logger { get; set; } = null!;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        public Config Configuration { get; set; } = null!;

        /// <summary>
        /// Configuration of this mod.
        /// </summary>
        public IModConfig ModConfig { get; set; } = null!;

        /// <summary>
        /// Instance of the IMod interface that created this mod instance.
        /// </summary>
        public IMod Owner { get; set; } = null!;
    }
}