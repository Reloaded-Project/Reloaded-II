using System;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModLoaderV1
    {
        /* Information */

        /// <summary>
        /// Retrieves the version of the mod loader.
        /// </summary>
        /// <remarks>
        ///     This function is provided in the possible case that it may be useful in the future.
        /// </remarks>
        Version GetLoaderVersion();

        /// <summary>
        /// Returns the individual application configuration for the currently running application.
        /// </summary>
        IApplicationConfigV1 GetAppConfig();

        /// <summary>
        /// Returns a list of all currently active mods.
        /// </summary>
        ModGenericTuple<IModConfigV1>[] GetActiveMods();

        /// <summary>
        /// Retrieves the logger.
        /// </summary>
        ILoggerV1 GetLogger();

        /* Events */

        /// <summary>
        /// This method is automatically called by the mod loader when all mods have finished loading.
        /// </summary>
        Action OnModLoaderInitialized { get; }
        
        /// <summary>
        /// This method is automatically called by the mod loader before a mod is unloaded.
        /// </summary>
        Action<IModV1, IModConfigV1> ModUnloading { get; }

        /// <summary>
        /// This method is automatically called by the mod loader before a mod is loaded.
        /// </summary>
        Action<IModV1, IModConfigV1> ModLoading { get; }

        /// <summary>
        /// This method is automatically called by the mod loader after a mod is loaded.
        /// </summary>
        Action<IModV1, IModConfigV1> ModLoaded { get; }

        /* Plugins and Extensibility */

        /// <summary>
        /// Visits all loaded mods and instantiates all classes which
        /// inherit from a given interface, returning all instances.
        /// </summary>
        /// <typeparam name="T">The type of the interface to instantiate.</typeparam>
        WeakReference<T>[] MakeInterfaces<T>() where T : class;

        /// <summary>
        /// Adds a controller to the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">The shared interface type of the controller.</typeparam>
        /// <param name="owner">The mod that owns the shared controller interface.</param>
        /// <param name="instance">The individual instance of the controller to share.</param>
        void AddOrReplaceController<T>(IModV1 owner, T instance);

        /// <summary>
        /// Removes a controller from the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">The shared interface type of the controller.</typeparam>
        void RemoveController<T>();

        /// <summary>
        /// Gets a collection of controllers from the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">Type of the controller to return.</typeparam>
        /// <returns>True if controller was found and value assigned. Else false.</returns>
        WeakReference<T> GetController<T>() where T : class;
    }

    /// <summary>
    /// Tuple that combines a modification and a given generic.
    /// </summary>
    public class ModGenericTuple<T>
    {
        public IModV1 Mod                  { get; set; }
        public T Generic                   { get; set; }

        public ModGenericTuple(IModV1 mod, T generic)
        {
            Mod = mod;
            Generic = generic;
        }
    }
}
