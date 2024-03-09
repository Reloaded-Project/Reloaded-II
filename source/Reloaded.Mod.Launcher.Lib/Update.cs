using Reloaded.Mod.Loader.Update.Providers.GitHub;
using Constants = Reloaded.Mod.Launcher.Lib.Misc.Constants;
using Version = Reloaded.Mod.Launcher.Lib.Utility.Version;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Contains static methods related to downloading loader, mods and updating them.
/// </summary>
public static class Update
{
    private static IEnumerable<ModConfig> _modLoaderDependencies = new ModConfig[]
    {
        new()
        {
            ModId = "reloaded.mod.loader",
            ModDependencies = new []{ "reloaded.sharedlib.hooks" },
            PluginData = new Dictionary<string, object>()
            {
                // GitHub Dependency Resolver
                {     
                    GitHubReleasesDependencyMetadataWriter.PluginId,
                    new DependencyResolverMetadata<GitHubReleasesUpdateResolverFactory.GitHubConfig>()
                    {
                        IdToConfigMap = new()
                        {
                            {
                                "reloaded.sharedlib.hooks",
                                new DependencyResolverItem<GitHubReleasesUpdateResolverFactory.GitHubConfig>()
                                {
                                    ReleaseMetadataName = "Sewer56.Update.ReleaseMetadata.json",
                                    Config = new GitHubReleasesUpdateResolverFactory.GitHubConfig()
                                    {
                                        RepositoryName = "Reloaded.SharedLib.Hooks.ReloadedII",
                                        UserName = "Sewer56",
                                        UseReleaseTag = true,
                                        AssetFileName = "reloaded.sharedlib.hooks.zip"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

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
        
        ModDependencyResolveResult? lastResolveResult = default;
        ModDependencyResolveResult resolveResult;

        do
        {
            resolveResult = await GetMissingDependenciesToDownload(token);
            if (resolveResult.FoundDependencies.Count <= 0)
                break;

            if (IsSameAsLast(resolveResult, lastResolveResult))
            {
                ShowStuckInDownloadLoopDialog(resolveResult);
                break;
            }

            lastResolveResult = resolveResult;
            DownloadPackages(resolveResult, token);
        } 
        while (true);

        if (resolveResult.NotFoundDependencies.Count > 0)
            ShowMissingPackagesDialog(resolveResult);
    }
    
    /// <summary>
    /// Resolves a list of missing packages.
    /// </summary>
    public static void ResolveMissingPackages()
    {
        if (!HasInternetConnection)
            return;
        
        ModDependencyResolveResult? lastResolveResult = default;
        ModDependencyResolveResult resolveResult;

        do
        {
            resolveResult = Task.Run(async () => await GetMissingDependenciesToDownload(default)).GetAwaiter().GetResult();
            if (resolveResult.FoundDependencies.Count <= 0)
                break;
            
            if (IsSameAsLast(resolveResult, lastResolveResult))
            {
                ShowStuckInDownloadLoopDialog(resolveResult);
                break;
            }

            DownloadPackages(resolveResult);
            lastResolveResult = resolveResult;
        } 
        while (true);

        if (resolveResult.NotFoundDependencies.Count > 0)
            ShowMissingPackagesDialog(resolveResult);
    }

    /// <summary>
    /// Displays the dialog indicating missing packages/dependencies.
    /// </summary>
    public static void ShowMissingPackagesDialog(ModDependencyResolveResult resolveResult)
    {
        // Note: This is slow, but it's ok in this rare case.
        var notFoundDeps = resolveResult.NotFoundDependencies;
        var list = new List<string>();
        var modConfigService = IoC.Get<ModConfigService>();
        
        foreach (var notFound in notFoundDeps)
        foreach (var item in modConfigService.Items)
        {
            var conf = item.Config;
            if (conf.ModDependencies.Contains(notFound)) 
                list.Add($"{notFound} | Required by: {conf.ModId}");
        }
        
        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
        {
            Actions.DisplayMessagebox(Resources.ErrorMissingDependency.Get(),
                $"{Resources.FetchNugetNotFoundMessage.Get()}\n\n" +
                $"{string.Join('\n', list)}\n\n" +
                $"{Resources.FetchNugetNotFoundAdvice.Get()}",
                new Actions.DisplayMessageBoxParams()
                {
                    Type = Actions.MessageBoxType.Ok,
                    StartupLocation = Actions.WindowStartupLocation.CenterScreen
                });
        });
    }

    /// <summary>
    ///     Gets all missing dependencies to be downloaded.
    /// </summary>
    public static async Task<ModDependencyResolveResult> GetMissingDependenciesToDownload(CancellationToken token)
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
        return ModDependencyResolveResult.Combine(results.Select(x => x.Result));;
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
        return modConfigService.GetMissingDependencies(_modLoaderDependencies);
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

    // TODO: This is a temporary hack to get people unstuck.
    private static bool IsSameAsLast(ModDependencyResolveResult thisItem, ModDependencyResolveResult? lastItem)
    {
        if (lastItem == null)
            return false;

        if (thisItem.FoundDependencies.Count != lastItem.FoundDependencies.Count)
            return false;

        // Assert whether they changed.
        // We will always have ID as we resolve deps by ID.
        var thisIds = new HashSet<string>(thisItem.FoundDependencies.Select(x => x.Id)!);
        var otherIds = new HashSet<string>(lastItem.FoundDependencies.Select(x => x.Id)!);
        return thisIds.SetEquals(otherIds);
    }

    private static void ShowStuckInDownloadLoopDialog(ModDependencyResolveResult result)
    {
        var message = new StringBuilder("We got stuck in a dependency download loop.\n" +
                                        "This bug is tracked at:\n" +
                                        "https://github.com/Reloaded-Project/Reloaded-II/issues/226\n\n" +
                                        "Here's a list of mods that's stuck:\n");

        foreach (var item in result.FoundDependencies)
        {
            message.AppendLine($"Id: {item.Id} | Name: {item.Name} | Version: {item.Version} | Source: {item.Source}");
        }

        message.AppendLine($"\nSometimes this can happen due to a mod incorrectly published/uploaded,\n" +
                           $"or a file being removed by a mod author of a dependency.\n\n" +
                           $"In some very rare cases, this can happen on any mod for completely unknown reasons.\n\n" +
                           $"Please report this issue to the link above if you encounter it.\n" +
                           $"In the meantime, download the required mods manually (you should " +
                           $"hopefully find it by ID or Name).\n\n" +
                           $"Sorry for the pain.");

        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
        {
            Actions.DisplayMessagebox.Invoke("Stuck in Download Loop", message.ToString(), new Actions.DisplayMessageBoxParams(){
                StartupLocation = Actions.WindowStartupLocation.CenterScreen
            });
        });
    }
}