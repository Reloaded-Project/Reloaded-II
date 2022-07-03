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
    /// <returns>A resolver that can handle the mod, else null.</returns>
    public static AggregatePackageProvider? GetProvider(PathTuple<ApplicationConfig> application)
    {
        // Create resolvers.
        var providers = new List<IDownloadablePackageProvider>();
        foreach (var factory in All)
        {
            var provider = factory.GetProvider(application);
            if (provider != null)
                providers.Add(provider);
        }

        return providers.Count > 0 ? new AggregatePackageProvider(providers.ToArray(), application.Config.AppName) : null;
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