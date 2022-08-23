using NuGet.Protocol;
using System;

namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Provides easy access to the details of mod update to be performed.
/// </summary>
public class ModUpdateSummary
{
    /// <summary>
    /// List of all pairs of resolvers, managers and update check results.
    /// </summary>
    public IList<ManagerModResultPair> ManagerModResultPairs { get; private set; }

    /// <summary>
    /// List of mods which failed to check updates for.
    /// </summary>
    public IList<ModConfig> FaultedMods { get; private set; }

    private ModUpdate[]? _updates = null!;

    /* Create summary. */

    /// <summary/>
    public ModUpdateSummary(IList<ManagerModResultPair> managerModResultPairs, IList<ModConfig> faultedMods)
    {
        ManagerModResultPairs = managerModResultPairs;
        FaultedMods = faultedMods;
    }

    /// <summary>
    /// Returns true if updates are available, else false.
    /// </summary>
    public bool HasUpdates()
    {
        return ManagerModResultPairs.Count > 0;
    }

    /// <summary>
    /// Retrieves info about the individual updates.
    /// </summary>
    /// <returns></returns>
    public ModUpdate[] GetUpdateInfo()
    {
        if (_updates != null) 
            return _updates;

        _updates = new ModUpdate[ManagerModResultPairs.Count];
        Task.Run(async () =>
        {
            for (var x = 0; x < ManagerModResultPairs.Count; x++)
            {
                var resultPairs = ManagerModResultPairs[x];
                var modName = resultPairs.ModTuple.Config.ModName;
                var modId = resultPairs.ModTuple.Config.ModId;
                var oldVersion = resultPairs.ModTuple.Config.ModVersion;
                var newVersion = resultPairs.Result.LastVersion;
                var resolver = ((IPackageResolverDownloadSize)resultPairs.Manager.Resolver);
                var updateSize = (long)0;
                string? changelog = null;

                try
                {
                    updateSize = await resolver.GetDownloadFileSizeAsync(newVersion!, resultPairs.ModTuple.GetVerificationInfo());
                }
                catch (Exception) { /* Ignored */ }

                // Get changelog from supported resolver.
                if (resolver is IPackageResolverGetLatestReleaseMetadata getMetadata)
                {
                    try
                    {
                        var releaseMetadata = await getMetadata.GetReleaseMetadataAsync(default);
                        var extraData = releaseMetadata?.GetExtraData<ReleaseMetadataExtraData>();
                        if (extraData != null)
                            changelog = extraData.Changelog;
                    }
                    catch (Exception) { /* Ignored */ }
                }

                // NuGet has special case, since it doesn't support release metadata but supports changelogs in nuspec.
                if (string.IsNullOrEmpty(changelog) && resolver is NuGetUpdateResolver nugetResolver)
                {
                    var copiedSettings = nugetResolver.GetResolverSettings();
                    var repository = NugetRepository.FromSourceUrl(copiedSettings.NugetRepository!.SourceUrl);
                    var reader = await repository.DownloadNuspecReaderAsync(new PackageIdentity(copiedSettings.PackageId, newVersion!));
                    if (reader != null)
                        changelog = reader?.GetReleaseNotes();
                }

                _updates[x] = new ModUpdate(modId, NuGetVersion.Parse(oldVersion), newVersion!, updateSize, changelog, modName);
            }

        }).Wait();

        return _updates;
    }

    /// <summary>
    /// Removes items to be updated by mod id.
    /// </summary>
    /// <param name="disabledModIds">IDs of mods to not update.</param>
    public void RemoveByModId(IEnumerable<string> disabledModIds)
    {
        var idToItemDict = ManagerModResultPairs.ToDictionary(pair => pair.ModTuple.Config.ModId);
        foreach (var disabledId in disabledModIds)
            idToItemDict.Remove(disabledId);

        ManagerModResultPairs = idToItemDict.Values.ToList();
    }
}