using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces
{
    public interface IExports
    {
        /// <summary>
        /// The types to be shared with other mods.
        /// i.e. All controller and plugin interfaces.
        /// </summary>
        Type[] GetTypes();
    }
}
