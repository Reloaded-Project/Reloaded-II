namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Provider that uses specified source to look for package dependencies.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// Tries to find results for a given package.
    /// </summary>
    /// <param name="packageId">ID of the package to resolve.</param>
    /// <param name="pluginData">May contain additional context for the resolver. This is usually obtained from <see cref="ModConfig.PluginData"/>.</param>
    /// <param name="token">Allows for the task to be canceled.</param>
    public Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default);
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

    /// <summary>
    /// Combines the results of multiple resolve operations.
    /// </summary>
    /// <param name="results">Results of multiple operations.</param>
    /// <returns>The result of combining multiple resolve operations.</returns>
    public static ModDependencyResolveResult Combine(IEnumerable<ModDependencyResolveResult> results)
    {
        var returnValue = new ModDependencyResolveResult();
        var idToNewestVersion = new Dictionary<string, IDownloadablePackage>();

        foreach (var result in results)
        {
            foreach (var found in result.FoundDependencies)
            {
                if (found.Id == null)
                    continue;

                if (idToNewestVersion.TryGetValue(found.Id, out var existing))
                {
                    if (existing.Version < found.Version)
                        idToNewestVersion[found.Id] = found;

                    continue;
                }
                
                idToNewestVersion[found.Id] = found;
            }

            foreach (var notFound in result.NotFoundDependencies)
                returnValue.NotFoundDependencies.Add(notFound);
        }

        // Remove dependencies that were found from the notFound set.
        foreach (var found in idToNewestVersion.Keys)
            returnValue.NotFoundDependencies.Remove(found);
        
        returnValue.FoundDependencies.AddRange(idToNewestVersion.Values);
        return returnValue;
    }
}