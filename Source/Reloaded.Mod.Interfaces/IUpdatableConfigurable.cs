using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces
{
    /// <summary>
    /// A variation of <see cref="IConfigurable"/> where the configurable itself can be updated.
    /// </summary>
    public interface IUpdatableConfigurable : IConfigurable
    {
        /// <summary>
        /// Automatically executed when the external configuration file is updated.
        /// Passes a new instance of the configuration as parameter.
        /// Inside your event handler, replace the variable storing the configuration with the new one.
        /// </summary>
        event Action<IUpdatableConfigurable> ConfigurationUpdated;
    }
}
