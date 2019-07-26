using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ModUpdateSummary
    {
        public List<ResolverManagerModResultPair> ResolverManagerResultPairs { get; private set; }
        private List<ModUpdate> _updates;

        /* Create summary. */
        public ModUpdateSummary(List<ResolverManagerModResultPair> resolverManagerResultPairs)
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
                    var modId = resultPairs.ModTuple.Object.ModId;
                    var oldVersion = resultPairs.Resolver.GetCurrentVersion();
                    var newVersion = resultPairs.Result.LastVersion;
                    var updateSize = resultPairs.Resolver.GetSize();

                    _updates.Add(new ModUpdate(modId, oldVersion, newVersion, updateSize));
                }
            }

            return _updates.ToArray();
        }

        /// <summary>
        /// Returns the size of all of the updates combined in bytes.
        /// </summary>
        public long GetTotalSize()
        {
            var updates = GetUpdateInfo();
            return updates.Sum(x => x.UpdateSize);
        }
    }
}
