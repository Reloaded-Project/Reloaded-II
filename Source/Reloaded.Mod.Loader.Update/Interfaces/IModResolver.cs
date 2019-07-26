using System;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.Update.Interfaces
{
    public interface IModResolver : IPackageResolver
    {
        /// <summary>
        /// Returns true if this mod is compatible to be updated. 
        /// </summary>
        /// <param name="mod">The mod to check.</param>
        /// <returns>True if this mod supports updating through this source.</returns>
        bool IsCompatible(PathGenericTuple<ModConfig> mod);

        /// <summary>
        /// Sets up this resolver to work with this mod.
        /// This will be called if IsCompatible == true.
        /// </summary>
        /// <param name="mod">The mod to set up the resolver for.</param>
        void Construct(PathGenericTuple<ModConfig> mod);

        /// <summary>
        /// Retrieves the current version of the mod.
        /// </summary>
        /// <returns>The current version of the mod.</returns>
        Version GetCurrentVersion();

        /// <summary>
        /// Retrieves the size of the download.
        /// </summary>
        /// <returns>The size of the download. If size cannot be determined, return -1.</returns>
        long GetSize();

        /// <summary>
        /// Called either after the update successfully completes or it is determined that there is no newer version available.
        /// </summary>
        /// <param name="hasUpdates">True if called after a successful update. Otherwise false if there are no updates to be made.</param>
        void PostUpdateCallback(bool hasUpdates);
    }
}
