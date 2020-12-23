using System.Collections.Generic;
using System.Diagnostics;

namespace Reloaded.Mod.Launcher.Utility.Structs
{
    public struct ProcessCollection
    {
        public List<Process> ReloadedProcesses;
        public List<Process> NonReloadedProcesses;

        public ProcessCollection(List<Process> reloadedProcesses, List<Process> nonReloadedProcesses)
        {
            ReloadedProcesses = reloadedProcesses;
            NonReloadedProcesses = nonReloadedProcesses;
        }

        public static ProcessCollection GetEmpty(int maxSize = 0)
        {
            return new ProcessCollection(new List<Process>(maxSize), new List<Process>(maxSize));
        }
    }
}
