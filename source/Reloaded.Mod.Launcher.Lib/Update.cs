using Constants = Reloaded.Mod.Launcher.Lib.Misc.Constants;
using Version = Reloaded.Mod.Launcher.Lib.Utility.Version;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Contains static methods related to downloading loader, mods and updating them.
/// </summary>
public static class Update
{
    /* Strings */

    /// <summary>
    /// True if the user has an internet connection, else false.
    /// </summary>
    public static bool HasInternetConnection { get; set; } = CheckForInternetConnection();
        
    /// <summary>
    /// Checks if there are any updates for the mod loader.
    /// </summary>
    public static async Task CheckForLoaderUpdatesAsync()
    {
        if (!HasInternetConnection)
            return;

        // Check for loader updates.
        UpdateManager<Empty>? manager = null;

        try
        {
            var releaseVersion = Version.GetReleaseVersion()!;
            var resolver = new GitHubReleaseResolver(new GitHubResolverConfiguration()
            {
                LegacyFallbackPattern = Constants.GitRepositoryReleaseName,
                RepositoryName = Constants.GitRepositoryName,
                UserName = Constants.GitRepositoryAccount
            }, new CommonPackageResolverSettings()
            {
                AllowPrereleases = releaseVersion.IsPrerelease
            });

            var metadata = new ItemMetadata(releaseVersion, Constants.ApplicationPath, null);
            manager  = await UpdateManager<Empty>.CreateAsync(metadata, resolver, new SevenZipSharpExtractor());

            // Check for new version and, if available, perform full update and restart
            var result = await manager.CheckForUpdatesAsync();
            if (result.CanUpdate)
            {
                Actions.SynchronizationContext.Send(_ =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    Actions.ShowModLoaderUpdateDialog(new ModLoaderUpdateDialogViewModel(manager, result.LastVersion!));
                }, null);
            }
        }
        catch (Exception ex)
        {
            manager?.Dispose();
            var errorMessage = $"{Resources.ErrorCheckUpdatesFailed.Get()}\n" +
                               $"{Resources.ErrorError.Get()}: {ex.Message}\n" +
                               $"{ex.StackTrace}";

            Actions.SynchronizationContext.Send(_ =>
            {
                Actions.DisplayMessagebox.Invoke(Resources.ErrorError.Get(), errorMessage, new Actions.DisplayMessageBoxParams()
                {
                    StartupLocation = Actions.WindowStartupLocation.CenterScreen
                });
            }, null);
        }
    }

    /// <summary>
    /// Checks if there are updates for any of the installed mods and/or new dependencies to fetch.
    /// </summary>
    public static async Task<bool> CheckForModUpdatesAsync()
    {
        if (!HasInternetConnection)
            return false;

        var loaderConfig = IoC.Get<LoaderConfig>();
        var modConfigService = IoC.Get<ModConfigService>();
        var modUserConfigService = IoC.Get<ModUserConfigService>();

        try
        {
            var nugetFeeds       = IoC.Get<AggregateNugetRepository>().Sources.Select(x => x.SourceUrl).ToList();
            var resolverSettings = new CommonPackageResolverSettings() { AllowPrereleases = loaderConfig.ForceModPrereleases };
            var updaterData      = new UpdaterData(nugetFeeds, resolverSettings);
            var updater          = new Updater(modConfigService, modUserConfigService, updaterData);
            var updateDetails    = await updater.GetUpdateDetailsAsync();

            if (updateDetails.HasUpdates())
            {
                Actions.SynchronizationContext.Send(_ =>
                {
                    Actions.ShowModUpdateDialog.Invoke(new ModUpdateDialogViewModel(updater, updateDetails));
                }, null);

                return true;
            }
        }
        catch (Exception e)
        {
            Actions.SynchronizationContext.Send(_ =>
            {
                Actions.DisplayMessagebox?.Invoke(Resources.ErrorError.Get(), e.Message + "|" + e.StackTrace, new Actions.DisplayMessageBoxParams()
                {
                    StartupLocation = Actions.WindowStartupLocation.CenterScreen
                });
            }, null);

            return false;
        }

        return false;
    }

    /// <summary>
    /// Resolves a list of missing packages.
    /// </summary>
    /// <param name="token">Used to cancel the operation.</param>
    public static async Task ResolveMissingPackagesAsync(CancellationToken token = default)
    {
        if (!HasInternetConnection)
            return;
        
        ModDependencyResolveResult resolveResult = null!;

        do
        {
            // Get missing dependencies for this update loop.
            var missingDeps = CheckMissingDependencies();

            // Get Dependencies
            var resolver = DependencyResolverFactory.GetInstance(IoC.Get<AggregateNugetRepository>());
            
            var results = new List<Task<ModDependencyResolveResult>>();
            foreach (var dependencyItem in missingDeps.Items)
            foreach (var dependency in dependencyItem.Dependencies)
                results.Add(resolver.ResolveAsync(dependency, dependencyItem.Mod.PluginData, token));

            await Task.WhenAll(results);

            // Merge Results
            resolveResult = ModDependencyResolveResult.Combine(results.Select(x => x.Result));
            DownloadPackages(resolveResult, token);
        } 
        while (resolveResult.FoundDependencies.Count > 0);

        if (resolveResult.NotFoundDependencies.Count > 0)
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                Actions.DisplayMessagebox(Resources.ErrorMissingDependency.Get(),
                    $"{Resources.FetchNugetNotFoundMessage.Get()}\n\n" +
                    $"{string.Join('\n', resolveResult.NotFoundDependencies)}\n\n" +
                    $"{Resources.FetchNugetNotFoundAdvice.Get()}",
                    new Actions.DisplayMessageBoxParams()
                    {
                        Type = Actions.MessageBoxType.Ok,
                        StartupLocation = Actions.WindowStartupLocation.CenterScreen
                    });
            });
        }
    }

    /// <summary>
    /// Shows the dialog for downloading dependencies given a result of dependency resolution.
    /// </summary>
    /// <param name="resolveResult">Result of resolving for missing packages.</param>
    /// <param name="token">Used to cancel the operation.</param>
    public static void DownloadPackages(ModDependencyResolveResult resolveResult, CancellationToken token = default)
    {
        if (!HasInternetConnection)
            return;

        if (resolveResult.FoundDependencies.Count <= 0)
            return;

        var viewModel  = new DownloadPackageViewModel(resolveResult.FoundDependencies, IoC.Get<LoaderConfig>());
        viewModel.Text = Resources.PackageDownloaderDownloadingDependencies.Get();

#pragma warning disable CS4014
        viewModel.StartDownloadAsync(); // Fire and forget.
#pragma warning restore CS4014
        Actions.SynchronizationContext.Send(state =>
        {
            Actions.ShowFetchPackageDialog.Invoke(viewModel);
        }, null);
    }

    /// <summary>
    /// Checks for all missing dependencies.
    /// </summary>
    /// <returns>True if there ar missing dependencies, else false.</returns>
    public static DependencyResolutionResult CheckMissingDependencies()
    {
        var modConfigService = IoC.Get<ModConfigService>();
        return modConfigService.GetMissingDependencies();
    }

    /// <summary>
    /// Checks if the user is connected to the internet using the same method Chromium OS does.
    /// </summary>
    /// <returns></returns>
    public static bool CheckForInternetConnection()
    {
        var urls = new List<string>()
        {
            "http://clients1.google.com/generate_204",
            "http://clients2.google.com/generate_204",
            "http://clients3.google.com/generate_204",
            "https://google.com",
            "https://github.com",
            "https://en.wikipedia.org",
            "https://baidu.com" // In case of Firewall of People's Republic of China.
        };

        foreach (var url in urls)
        {
            try
            {
                using var client = new WebClient();
                using (client.OpenRead(url))
                    return true;
            }
            catch
            {
                // ignored
            }
        }

        return false;
    }
}