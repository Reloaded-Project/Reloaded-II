using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Reloaded.Mod.Interfaces
{
    /// <summary>
    /// Interface that represents an individual configuration.
    /// Instances of this interface <see cref="IConfigurable"/> should be created by the <see cref="IConfigurator"/>.
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Returns the name of the configuration.
        /// </summary>
        string ConfigName { get; }

        /// <summary>
        /// Saves the current configuration.
        /// </summary>
        Action Save { get; }
    }
}
