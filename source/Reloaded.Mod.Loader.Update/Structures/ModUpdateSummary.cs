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
    /// Synchronous wrapper; do not call from the UI thread. Prefer <see cref="GetUpdateInfoAsync"/>.
    /// </summary>
    /// <returns></returns>
    public ModUpdate[] GetUpdateInfo() => Task.Run(GetUpdateInfoAsync).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves info about the individual updates without blocking the calling thread.
    /// </summary>
    /// <returns></returns>
    public async Task<ModUpdate[]> GetUpdateInfoAsync()
    {
        if (_updates != null) 
            return _updates;

        var updates = new ModUpdate[ManagerModResultPairs.Count];
        for (var x = 0; x < ManagerModResultPairs.Count; x++)
        {
            var resultPairs = ManagerModResultPairs[x];
            var modName = resultPairs.ModTuple.Config.ModName;
            var modId = resultPairs.ModTuple.Config.ModId;
            var oldVersion = resultPairs.ModTuple.Config.ModVersion;
            var newVersion = resultPairs.Result.LastVersion;
            var resolver = resultPairs.Manager.Resolver;
            var updateSize = (long)0;
            string? changelog = null;

            if (resolver is IPackageResolverDownloadSize hasDownloadSize)
            {
                try
                {
                    updateSize = await hasDownloadSize.GetDownloadFileSizeAsync(newVersion!, resultPairs.ModTuple.GetVerificationInfo()).ConfigureAwait(false);
                }
                catch (Exception) { /* Ignored */ }
            }

            // Get changelog from supported resolver.
            if (resolver is IPackageResolverGetLatestReleaseMetadata getMetadata)
            {
                try
                {
                    var releaseMetadata = await getMetadata.GetReleaseMetadataAsync(default).ConfigureAwait(false);
                    var extraData = releaseMetadata?.GetExtraData<ReleaseMetadataExtraData>();
                    if (extraData != null)
                        changelog = extraData.Changelog;
                }
                catch (Exception) { /* Ignored */ }
            }

            // NuGet has special case, since it doesn't support release metadata but supports changelogs in nuspec.
            if (string.IsNullOrEmpty(changelog) && resolver is NuGetUpdateResolver nugetResolver)
            {
                try
                {
                    var copiedSettings = nugetResolver.GetResolverSettings();
                    var repository = NugetRepository.FromSourceUrl(copiedSettings.NugetRepository!.SourceUrl);
                    var reader = await repository.DownloadNuspecReaderAsync(new PackageIdentity(copiedSettings.PackageId, newVersion!)).ConfigureAwait(false);
                    if (reader != null)
                        changelog = reader?.GetReleaseNotes();
                }
                catch (Exception) { /* Ignored */ }
            }

            updates[x] = new ModUpdate(modId, NuGetVersion.Parse(oldVersion), newVersion!, updateSize, changelog, modName);
        }

        _updates = updates;
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