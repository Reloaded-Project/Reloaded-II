using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Onova;
using Onova.Models;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Loader.Update
{
    /// <summary>
    /// Class that provides easy update support for mods.
    /// </summary>
    public class Updater
    {
        private IEnumerable<PathGenericTuple<ModConfig>>    _mods;
        private List<ResolverManagerModResultPair>          _resolversWithUpdates;

        /* Instantiation */
        public Updater(IEnumerable<PathGenericTuple<ModConfig>> mods)
        {
            _mods = mods;
        }

        /* Get resolvers. */

        /// <summary>
        /// Retrieves a summary of all updates to be performed.
        /// </summary>
        public async Task<ModUpdateSummary> GetUpdateDetails()
        {
            if (_resolversWithUpdates == null)
            {
                var resolverManagerPairs = new List<ResolverManagerModResultPair>();
                var resolverTuples = GetResolvers();
                foreach (var resolverTuple in resolverTuples)
                {
                    var modTuple = resolverTuple.ModTuple;
                    var version  = resolverTuple.Resolver.GetCurrentVersion();

                    // Note: Since R2R Support was added, we cannot predict which DLL will be in use. Just return the config file as main file.
                    string filePath = modTuple.Path;
                    var metadata    = new AssemblyMetadata(PathSanitizer.ForceValidFilePath(modTuple.Object.ModName), version, filePath);

                    var manager         = new UpdateManager(metadata, resolverTuple.Resolver, resolverTuple.Resolver.Extractor);
                    var updateResult    = await manager.CheckForUpdatesAsync();

                    if (updateResult.CanUpdate)
                        resolverManagerPairs.Add(new ResolverManagerModResultPair(resolverTuple.Resolver, manager, updateResult, modTuple));
                    else
                        resolverTuple.Resolver.PostUpdateCallback(false);
                }

                _resolversWithUpdates = resolverManagerPairs;
                return new ModUpdateSummary(_resolversWithUpdates);
            }

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
            int pairCount = summary.ResolverManagerResultPairs.Count;

            var completeMods                = new Box<int>(0);
            float completeModSegmentLength  = (float) 1 / pairCount;
            Progress<double> totalProgressHandler = null;

            if (progressHandler != null)
            {
                totalProgressHandler = new Progress<double>(progress =>
                {
                    progressHandler.Report((progress / pairCount) + (completeModSegmentLength * completeMods.Value));
                });
            }

            foreach (var pair in summary.ResolverManagerResultPairs)
            {
                try
                {
                    var manager = pair.Manager;
                    var version = pair.Result.LastVersion;
                    await manager.PrepareUpdateAsync(version, totalProgressHandler);
                    manager.LaunchUpdater(version, false);
                    pair.Resolver.PostUpdateCallback(true);
                    completeMods.Value += 1;
                }
                catch (Exception)
                {
                    Debugger.Break();
                }
            }
        }

        private List<ResolverModPair> GetResolvers()
        {
            var modResolverPairs = new List<ResolverModPair>();
            foreach (var mod in _mods)
            {
                var resolver = ResolverFactory.GetResolver(mod);
                if (resolver != null)
                {
                    resolver.Construct(mod);
                    modResolverPairs.Add(new ResolverModPair(resolver, mod));
                }
            }

            return modResolverPairs;
        }

        private class Box<T>
        {
            public T Value { get; set; }
            public Box(T value)
            {
                Value = value;
            }
        }
    }
}
