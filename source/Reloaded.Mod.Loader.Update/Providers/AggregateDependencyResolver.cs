namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Dependency resolver that combines multiple sources to resolve a single package.
/// </summary>
public class AggregateDependencyResolver : IDependencyResolver
{
    private IDependencyResolver[] _resolvers;

    /// <summary/>
    public AggregateDependencyResolver(IDependencyResolver[] resolvers)
    {
        _resolvers = resolvers;
    }

    /// <inheritdoc />
    public async Task<ModDependencyResolveResult> ResolveAsync(string packageId, Dictionary<string, object>? pluginData = null, CancellationToken token = default)
    {
        // Run parallel resolve operations
        var tasks = new Task<ModDependencyResolveResult>[_resolvers.Length];
        for (var x = 0; x < _resolvers.Length; x++)
            tasks[x] = _resolvers[x].ResolveAsync(packageId, pluginData, token);

        await Task.WhenAll(tasks);

        // Merge results.
        var result = new ModDependencyResolveResult();
        var packageToVersionMap = new Dictionary<string, IDownloadablePackage>();
        
        foreach (var task in tasks)
        {
            // Merge found dependencies
            foreach (var dependency in task.Result.FoundDependencies)
            {
                if (dependency.Id == null)
                    continue;

                if (!packageToVersionMap.TryGetValue(dependency.Id, out var existing))
                {
                    packageToVersionMap[dependency.Id] = dependency;
                    continue;
                }

                if (dependency.Version > existing.Version)
                    packageToVersionMap[dependency.Id] = dependency;
            }

            // Merge not found dependencies.
            foreach (var notFound in task.Result.NotFoundDependencies)
                result.NotFoundDependencies.Add(notFound);
        }

        result.FoundDependencies.AddRange(packageToVersionMap.Values);
        return result;
    }
}