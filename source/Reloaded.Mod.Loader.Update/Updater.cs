using Polly;

namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Class that provides easy update support for mods.
/// </summary>
public class Updater
{
    private ModConfigService _modConfigService;
    private ModUserConfigService _modUserConfigService;

    private ModUpdateSummary? _cachedResult;
    private UpdaterData _data;

    /* Instantiation */

    /// <summary/>
    /// <param name="mods">List of mods to be updated. Usually obtained from <see cref="ModConfigService"/>.</param>
    /// <param name="userConfigurations">List of user configurations. Usually obtained from <see cref="ModUserConfigService"/>.</param>
    /// <param name="data">Configuration for the updater.</param>
    public Updater(ModConfigService mods, ModUserConfigService userConfigurations, UpdaterData data)
    {
        _modConfigService = mods;
        _modUserConfigService = userConfigurations;
        _data = data;
    }

    /* Get resolvers. */

    /// <summary>
    /// Retrieves a summary of all updates to be performed.
    /// </summary>
    public async Task<ModUpdateSummary> GetUpdateDetailsAsync()
    {
        if (_cachedResult != null)
            return _cachedResult;

        var modCount = Math.Max(1, _modConfigService.Items.Count);
        var faultedModSets       = new BlockingCollection<ModConfig>(modCount);
        var resolverManagerPairs = new BlockingCollection<ManagerModResultPair>(modCount);
        var resolverTuples       = GetResolvers();
        using var concurrencySemaphore = new SemaphoreSlim(32);
        var allTasks = new List<Task>();

        foreach (var resolverTuple in resolverTuples)
        {
            await concurrencySemaphore.WaitAsync();
            var task = CheckForResolverTupleUpdate(resolverTuple, resolverManagerPairs, faultedModSets).ContinueWith(task1 =>
            {
                concurrencySemaphore.Release();
            });

            allTasks.Add(task);
        }

        await Task.WhenAll(allTasks);
        _cachedResult = new ModUpdateSummary(resolverManagerPairs.ToList(), faultedModSets.ToList());
        return _cachedResult;
    }

    private static async Task CheckForResolverTupleUpdate(ResolverModPair resolverTuple, BlockingCollection<ManagerModResultPair> toUpdateSet, BlockingCollection<ModConfig> faultedUpdateSets)
    {
        try
        {
            var modTuple = resolverTuple.ModTuple;
            var filePath = modTuple.Config.GetDllPath(modTuple.Path);
            var baseDirectory = Path.GetDirectoryName(modTuple.Path);

            var metadata = new ItemMetadata(NuGetVersion.Parse(modTuple.Config.ModVersion), filePath, baseDirectory);
            var manager = await UpdateManager<Empty>.CreateAsync(metadata, resolverTuple.Resolver, resolverTuple.Resolver.Extractor);
            var updateResult = await manager.CheckForUpdatesAsync();

            if (updateResult.CanUpdate)
                toUpdateSet.Add(new ManagerModResultPair(manager, updateResult, modTuple));
        }
        catch (Exception)
        {
            faultedUpdateSets.Add(resolverTuple.ModTuple.Config);
        }
    }

    /// <summary>
    /// Updates all of the mods.
    /// </summary>
    /// <param name="summary">Summary of updates returned from <see cref="GetUpdateDetailsAsync"/></param>
    /// <param name="progressHandler">Event to receive information about the overall download and extract progress of all mods combined.</param>
    public async Task Update(ModUpdateSummary summary, IProgress<double>? progressHandler = null)
    {
        // Guard against null.
        var progressMixer      = new ProgressSlicer(progressHandler);
        var singleItemProgress = 1.0 / summary.ManagerModResultPairs.Count;

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
        // Note: We don't have a 'ForEachAsync' in .NET 5
        await Task.Run(() =>
        {
            Parallel.ForEach(summary.ManagerModResultPairs, parallelOptions, pair =>
            {
                var slice = progressMixer.Slice(singleItemProgress);
                var manager = pair.Manager;
                var version = pair.Result.LastVersion;

                retryPolicy.ExecuteAsync(async () =>
                {
                    await manager.PrepareUpdateAsync(version!, slice);
                    await manager.StartUpdateAsync(version!, new OutOfProcessOptions(), new UpdateOptions() { CleanupAfterUpdate = true });
                    manager.Dispose();
                }).GetAwaiter().GetResult();
            });
        });
    }

    private List<ResolverModPair> GetResolvers()
    {
        var modResolverPairs = new List<ResolverModPair>();
        foreach (var mod in _modConfigService.Items.ToArray())
        {
            _modUserConfigService.ItemsById.TryGetValue(mod.Config.ModId, out var userConfig);
            var resolver = PackageResolverFactory.GetResolver(mod, userConfig, _data);
            if (resolver != null)
                modResolverPairs.Add(new ResolverModPair(resolver, mod));
        }

        return modResolverPairs;
    }
}