using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

namespace Reloaded.Mod.Loader.Update.Structures
{
    /// <summary>
    /// Represents all of the data passed to the repository.
    /// </summary>
    public class UpdaterData
    {
        /// <summary>
        /// Contains the repository data.
        /// </summary>
        public AggregateNugetRepository AggregateNugetRepository { get; set; }

        public UpdaterData(AggregateNugetRepository aggregateNugetRepository)
        {
            AggregateNugetRepository = aggregateNugetRepository;
        }
    }
}
