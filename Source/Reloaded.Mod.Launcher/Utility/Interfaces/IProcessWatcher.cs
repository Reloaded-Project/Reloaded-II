using System.Diagnostics;

namespace Reloaded.Mod.Launcher.Utility.Interfaces
{
    public interface IProcessWatcher
    {
        event ProcessArrived OnNewProcess;
        event ProcessExited OnRemovedProcess;
    }

    public delegate void ProcessArrived(Process newProcess);
    public delegate void ProcessExited(int processId);
}