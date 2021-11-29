using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Utilities;
using Sewer56.Update.Interfaces.Extensions;
using Sewer56.Update.Packaging.Structures;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ModUpdateSummary
    {
        public List<ManagerModResultPair> ResolverManagerResultPairs { get; private set; }
        private List<ModUpdate> _updates;

        /* Create summary. */
        public ModUpdateSummary(List<ManagerModResultPair> resolverManagerResultPairs)
        {
            ResolverManagerResultPairs = resolverManagerResultPairs;
        }

        /// <summary>
        /// Returns true if updates are available, else false.
        /// </summary>
        public bool HasUpdates()
        {
            return ResolverManagerResultPairs.Count > 0;
        }

        /// <summary>
        /// Retrieves info about the individual updates.
        /// </summary>
        /// <returns></returns>
        public ModUpdate[] GetUpdateInfo()
        {
            if (_updates == null)
            {
                _updates = new List<ModUpdate>();
                foreach (var resultPairs in ResolverManagerResultPairs)
                {
                    var modId = resultPairs.ModTuple.Config.ModId;
                    var oldVersion = resultPairs.ModTuple.Config.ModVersion;
                    var newVersion = resultPairs.Result.LastVersion;
                    var resolver   = ((IPackageResolverDownloadSize)resultPairs.Manager.Resolver);
                    var updateSize = Task.Run(() => resolver.GetDownloadFileSizeAsync(resultPairs.Result.LastVersion, resultPairs.ModTuple.GetVerificationInfo())).Result;

                    _updates.Add(new ModUpdate(modId, NuGetVersion.Parse(oldVersion), newVersion, updateSize));
                }
            }

            return _updates.ToArray();
        }
    }
}
