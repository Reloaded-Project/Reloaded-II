using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Versioning;
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
    public List<ManagerModResultPair> ManagerModResultPairs { get; private set; }
    private List<ModUpdate> _updates;

    /* Create summary. */

    /// <summary/>
    public ModUpdateSummary(List<ManagerModResultPair> managerModResultPairs)
    {
        ManagerModResultPairs = managerModResultPairs;
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
            return _updates.ToArray();

        _updates = new List<ModUpdate>();
        foreach (var resultPairs in ManagerModResultPairs)
        {
            var modId = resultPairs.ModTuple.Config.ModId;
            var oldVersion = resultPairs.ModTuple.Config.ModVersion;
            var newVersion = resultPairs.Result.LastVersion;
            var resolver   = ((IPackageResolverDownloadSize)resultPairs.Manager.Resolver);
            var updateSize = Task.Run(() => resolver.GetDownloadFileSizeAsync(resultPairs.Result.LastVersion, resultPairs.ModTuple.GetVerificationInfo())).Result;

            _updates.Add(new ModUpdate(modId, NuGetVersion.Parse(oldVersion), newVersion, updateSize));
        }

        return _updates.ToArray();
    }
}