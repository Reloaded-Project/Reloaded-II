using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Reloaded.Mod.Loader.Update.Providers.NuGet;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

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
        return new AggregateDependencyResolver(new IDependencyResolver[]
        {
            new NuGetDependencyResolver(repository),
            new GameBananaDependencyResolver()
        });
    }
}