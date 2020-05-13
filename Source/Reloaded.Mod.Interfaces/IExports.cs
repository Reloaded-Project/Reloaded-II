using System;

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
