using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Loader.IO.Interfaces
{
    public interface IConfigCleanup
    {
        /// <summary>
        /// Verifies the validity of the configuration, removing/replacing any items that are invalid or may not exist.
        /// </summary>
        void CleanupConfig();
    }
}
