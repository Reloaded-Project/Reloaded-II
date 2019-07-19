using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Loader.Server
{
    public static class ServerUtility
    {
        public static string GetMappedFileNameForPid(int pid) => $"Reloaded-Mod-Loader-Server-PID-{pid}";
    }
}
