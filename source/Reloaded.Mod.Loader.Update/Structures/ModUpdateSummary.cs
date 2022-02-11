using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Utilities;
using Sewer56.Update.Interfaces.Extensions;

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
        for (var x = 0; x < ManagerModResultPairs.Count; x++)
        {
            var resultPairs = ManagerModResultPairs[x];
            var modId = resultPairs.ModTuple.Config.ModId;
            var oldVersion = resultPairs.ModTuple.Config.ModVersion;
            var newVersion = resultPairs.Result.LastVersion;
            var resolver = ((IPackageResolverDownloadSize)resultPairs.Manager.Resolver);
            var updateSize = (long)0;

            try
            {
                updateSize = Task.Run(async () => await resolver.GetDownloadFileSizeAsync(newVersion!, resultPairs.ModTuple.GetVerificationInfo())).Result;
            }
            catch (Exception) { /* */ }

            _updates[x] = new ModUpdate(modId, NuGetVersion.Parse(oldVersion), newVersion!, updateSize);
        }

        return _updates;
    }
}