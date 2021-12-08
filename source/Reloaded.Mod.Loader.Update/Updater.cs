using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Class that provides easy update support for mods.
/// </summary>
public class Updater
{
    private ModConfigService _modConfigService;
    private ModUserConfigService _modUserConfigService;
    private List<ManagerModResultPair> _resolversWithUpdates;
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
    public async Task<ModUpdateSummary> GetUpdateDetails()
    {
        if (_resolversWithUpdates != null) 
            return new ModUpdateSummary(_resolversWithUpdates);
            
        // TODO: Work from Here!
        var resolverManagerPairs = new List<ManagerModResultPair>();
        var resolverTuples       = GetResolvers();
        var extractor            = new SevenZipSharpExtractor();

        foreach (var resolverTuple in resolverTuples)
        {
            var modTuple = resolverTuple.ModTuple;
            var filePath = modTuple.Config.GetDllPath(modTuple.Path);
            var baseDirectory = Path.GetDirectoryName(modTuple.Path);

            var metadata     = new ItemMetadata(NuGetVersion.Parse(modTuple.Config.ModVersion), filePath, baseDirectory);
            var manager      = await UpdateManager<Empty>.CreateAsync(metadata, resolverTuple.Resolver, extractor);
            var updateResult = await manager.CheckForUpdatesAsync();

            if (updateResult.CanUpdate)
                resolverManagerPairs.Add(new ManagerModResultPair(manager, updateResult, modTuple));
        }

        _resolversWithUpdates = resolverManagerPairs;
        return new ModUpdateSummary(_resolversWithUpdates);
    }

    /// <summary>
    /// Updates all of the mods.
    /// </summary>
    /// <param name="summary">Summary of updates returned from <see cref="GetUpdateDetails"/></param>
    /// <param name="progressHandler">Event to receive information about the overall download and extract progress of all mods combined.</param>
    public async Task Update(ModUpdateSummary summary, IProgress<double> progressHandler = null)
    {
        // Guard against null.
        var progressMixer      = new ProgressSlicer(progressHandler);
        var singleItemProgress = 1.0 / summary.ManagerModResultPairs.Count;

        for (var x = 0; x < summary.ManagerModResultPairs.Count; x++)
        {
            var slice   = progressMixer.Slice(singleItemProgress);
            var pair    = summary.ManagerModResultPairs[x];
            var manager = pair.Manager;
            var version = pair.Result.LastVersion;

            await manager.PrepareUpdateAsync(version, slice);
            await manager.StartUpdateAsync(version, new OutOfProcessOptions(), new UpdateOptions() { CleanupAfterUpdate = true });
        }
    }

    private List<ResolverModPair> GetResolvers()
    {
        var modResolverPairs = new List<ResolverModPair>();
        foreach (var mod in _modConfigService.Items.ToArray())
        {
            _modUserConfigService.ItemsById.TryGetValue(mod.Config.ModId, out var userConfig);
            var resolver = ResolverFactory.GetResolver(mod, userConfig, _data);
            if (resolver != null)
                modResolverPairs.Add(new ResolverModPair(resolver, mod));
        }

        return modResolverPairs;
    }
}