using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Provider that uses specified source to look for package dependencies.
/// </summary>
public interface IModDependencyPackageProvider
{
    /// <summary>
    /// Tries to find results for a given package.
    /// </summary>
    /// <param name="packageId">ID of the package to resolve.</param>
    /// <param name="modConfig">Mod configuration of the mod expressing this ID as dependency. May be null.</param>
    /// <param name="token">Allows for the task to be canceled.</param>
    public Task<ModDependencyResolveResult> ResolveAsync(string packageId, ModConfig? modConfig = null, CancellationToken token = default);
}

/// <summary>
/// Result of a mod dependency search.
/// </summary>
public class ModDependencyResolveResult
{
    /// <summary>
    /// List of all dependencies that were found.
    /// </summary>
    public List<IDownloadablePackage> FoundDependencies { get; } = new List<IDownloadablePackage>();

    /// <summary>
    /// List of all dependencies that were not found.
    /// </summary>
    public HashSet<string> NotFoundDependencies { get; } = new HashSet<string>();
}