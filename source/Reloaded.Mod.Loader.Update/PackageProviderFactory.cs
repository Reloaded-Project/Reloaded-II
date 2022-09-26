namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Builds an <see cref="AggregatePackageProvider"/> using all configured individual provider factories.
/// </summary>
public static class PackageProviderFactory
{
    /// <summary>
    /// List of all supported factories in preference order.
    /// </summary>
    public static IPackageProviderFactory[] All { get; private set; } =
    {
        // Listed in order of preference.
        new GameBananaPackageProviderFactory()
    };

    /// <summary>
    /// Returns the first appropriate provider that can handle fetching packages for a game.
    /// </summary>
    /// <param name="application">The application in question.</param>
    /// <param name="nugetRepositories">[Optional] NuGet repositories to use.</param>
    /// <returns>A resolver that can handle the mod, else null.</returns>
    public static AggregatePackageProvider? GetProvider(PathTuple<ApplicationConfig> application, IEnumerable<INugetRepository>? nugetRepositories = null)
    {
        // Create resolvers.
        var providers = new List<IDownloadablePackageProvider>();
        foreach (var factory in All)
        {
            var provider = factory.GetProvider(application);
            if (provider != null)
                providers.Add(provider);
        }

        // Add NuGets.
        if (nugetRepositories != null)
            foreach (var nugetRepo in nugetRepositories)
                providers.Add(new IndexedNuGetPackageProvider(nugetRepo, application.Config.AppId));

        return providers.Count > 0 ? new AggregatePackageProvider(providers.ToArray(), application.Config.AppName) : null;
    }

    /// <summary>
    /// Gets all possible providers for downloadable packages.
    /// </summary>
    /// <param name="applications">Applications for which to get provider for.</param>
    /// <param name="repositories">Repositories which to include.</param>
    /// <returns>List of providers of downloadable packages.</returns>
    public static List<IDownloadablePackageProvider> GetAllProviders(IEnumerable<PathTuple<ApplicationConfig>> applications, IEnumerable<INugetRepository>? repositories = null)
    {
        var providers = new List<IDownloadablePackageProvider>();
        
        // Get package provider for individual games.
        var repos = new List<INugetRepository>();
        if (repositories != null)
            repos.AddRange(repositories);
        
        foreach (var appConfig in applications)
        {
            var provider = GetProvider(appConfig, repos);
            if (provider != null)
                providers.Add(provider);
        }

        // Get package provider for all packages.
        foreach (var nugetSource in repos)
            providers.Add(new IndexedNuGetPackageProvider(nugetSource));

        return providers;
    }

    /// <summary>
    /// TEST USE ONLY.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetProviderFactories(IPackageProviderFactory[] factories)
    {
        All = factories;
    }
}