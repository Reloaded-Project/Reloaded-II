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
        IApplicationConfig GetAppConfig();

        /// <summary>
        /// Returns a list of all currently active mods.
        /// </summary>
        (IMod, IModConfig)[] GetActiveMods();

        /// <summary>
        /// Retrieves the logger.
        /// </summary>
        ILogger GetLogger();

        /* Events */

        /// <summary>
        /// This method is automatically called by the mod loader when all mods have finished loading.
        /// </summary>
        Action OnModLoaderInitialized { get; }
        
        /// <summary>
        /// This method is automatically called by the mod loader before a mod is unloaded.
        /// </summary>
        Action<IMod, IModConfig> ModUnloading { get; }

        /// <summary>
        /// This method is automatically called by the mod loader before a mod is loaded.
        /// </summary>
        Action<IMod, IModConfig> ModLoading { get; }

        /// <summary>
        /// This method is automatically called by the mod loader after a mod is unloaded.
        /// </summary>
        Action<IMod, IModConfig> ModUnloaded { get; }

        /// <summary>
        /// This method is automatically called by the mod loader after a mod is loaded.
        /// </summary>
        Action<IMod, IModConfig> ModLoaded { get; }

        /* Plugins and Extensibility */

        /// <summary>
        /// Visits all loaded mods and instantiates all classes which
        /// inherit from a given interface, returning all instances.
        /// </summary>
        /// <typeparam name="T">The type of the interface to instantiate.</typeparam>
        (IMod, T)[] MakeInterfaces<T>();

        /// <summary>
        /// Adds a controller to the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">The shared interface type of the controller.</typeparam>
        /// <param name="instance">The individual instance of the controller to share.</param>
        void AddController<T>(T instance);

        /// <summary>
        /// Removes a controller from the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">The shared interface type of the controller.</typeparam>
        /// <param name="instance">The individual instance of the controller to remove.</param>
        void RemoveController<T>(T instance);

        /// <summary>
        /// Gets a collection of controllers from the mod loader's stored list of controllers.
        /// </summary>
        /// <typeparam name="T">Type of the controller to return.</typeparam>
        T[] GetController<T>();
    }
}
