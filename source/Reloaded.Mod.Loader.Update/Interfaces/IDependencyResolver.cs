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
    /// List of errors that occurred during dependency resolution.
    /// </summary>
    public List<DependencyResolveError> Errors { get; } = new List<DependencyResolveError>();

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
                
            foreach (var error in result.Errors)
                returnValue.Errors.Add(error);
        }

        // Remove dependencies that were found from the notFound set.
        foreach (var found in idToNewestVersion.Keys)
            returnValue.NotFoundDependencies.Remove(found);
        
        returnValue.FoundDependencies.AddRange(idToNewestVersion.Values);
        return returnValue;
    }

    /// <summary>
    /// Creates a result with an error for a specific package.
    /// </summary>
    /// <param name="packageId">The package ID that failed to resolve.</param>
    /// <param name="exception">The exception that occurred during resolution.</param>
    /// <param name="resolver">The resolver that caused the error.</param>
    /// <returns>A result containing the error information.</returns>
    public static ModDependencyResolveResult FromError(string packageId, Exception exception, string resolver)
    {
        var result = new ModDependencyResolveResult();
        result.Errors.Add(new DependencyResolveError(packageId, exception, resolver));
        result.NotFoundDependencies.Add(packageId);
        return result;
    }
}

/// <summary>
/// Represents an error that occurred during dependency resolution.
/// </summary>
public class DependencyResolveError
{
    /// <summary>
    /// The package ID that failed to resolve.
    /// </summary>
    public string PackageId { get; }

    /// <summary>
    /// The exception that occurred during resolution.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// The name of the resolver that caused the error.
    /// </summary>
    public string Resolver { get; }

    /// <summary>
    /// Creates a new dependency resolve error.
    /// </summary>
    /// <param name="packageId">The package ID that failed to resolve.</param>
    /// <param name="exception">The exception that occurred during resolution.</param>
    /// <param name="resolver">The name of the resolver that caused the error.</param>
    public DependencyResolveError(string packageId, Exception exception, string resolver)
    {
        PackageId = packageId;
        Exception = exception;
        Resolver = resolver;
    }
}