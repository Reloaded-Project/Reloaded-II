using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// An interface that can be used to optionally return a resolver instance.
/// </summary>
public interface IResolverFactory
{
    /// <summary>
    /// Returns the unique ID of the resolver.
    /// </summary>
    string ResolverId { get; }

    /// <summary>
    /// Migrates (if necessary) from an earlier version of the resolver.
    /// </summary>
    /// <param name="mod">The mod to check.</param>
    /// <param name="userConfig">User specific configuration for the given mod.</param>
    void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig);

    /// <summary>
    /// Returns true if this mod is compatible to be updated. 
    /// </summary>
    /// <param name="mod">The mod to check.</param>
    /// <param name="data">Various configurations and general data provided to all the updaters.</param>
    /// <param name="userConfig">User specific configuration for the given mod.</param>
    /// <returns>A valid resolver if the mod can update through it, else false.</returns>
    IPackageResolver GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig, UpdaterData data);
}