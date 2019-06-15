using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reloaded.Mod.Launcher.Utility
{
    public static class ActionWrappers
    {
        public static void TryCatch(Action action)
        {
            try { action(); }
            catch (Exception) { /* ignored */ }
        }
    }
}
