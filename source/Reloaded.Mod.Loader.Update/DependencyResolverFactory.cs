using Reloaded.Mod.Loader.Update.Providers.Index;

namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Builds an <see cref="AggregateDependencyResolver"/> using all pre-configured individual dependency resolvers.
/// </summary>
public static class DependencyResolverFactory
{
    /// <summary>
    /// Creates an instance of an aggregate resolver.
    /// </summary>
    public static IDependencyResolver GetInstance(AggregateNugetRepository repository)
    {
        return new AggregateDependencyResolver([
            new NuGetDependencyResolver(repository),
            new GameBananaDependencyResolver(),
            new GitHubDependencyResolver(),
            new IndexDependencyResolver()
        ]);
    }
}