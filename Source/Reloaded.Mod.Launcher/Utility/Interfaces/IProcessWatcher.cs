using System;
using System.Diagnostics;

namespace Reloaded.Mod.Launcher.Utility.Interfaces
{
    public interface IProcessWatcher
    {
        event ProcessArrived OnNewProcess;
        event ProcessExited OnRemovedProcess;

        private static IProcessWatcher _watcher;
        public static IProcessWatcher Get()
        {
            if (_watcher == null)
            {
                // On Wine (and modified Windows OS) WMI might not work.
                try { _watcher = App.IsElevated ? (IProcessWatcher)new WmiProcessWatcher() : ProcessWatcher.Instance; }
                catch (Exception) { _watcher = ProcessWatcher.Instance; }
            }

            return _watcher;
        }
    }

    public delegate void ProcessArrived(Process newProcess);
    public delegate void ProcessExited(int processId);
}