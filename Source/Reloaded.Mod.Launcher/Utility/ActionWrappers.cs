using System;

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
