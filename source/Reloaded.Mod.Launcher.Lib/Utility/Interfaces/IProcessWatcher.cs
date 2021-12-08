using System;
using System.Diagnostics;

namespace Reloaded.Mod.Launcher.Lib.Utility.Interfaces;

/// <summary>
/// Interface used to watch for the launching and exiting of processes.
/// </summary>
public interface IProcessWatcher
{
    /// <summary>
    /// Executed when a new process has been launched.
    /// </summary>
    event ProcessArrived OnNewProcess;

    /// <summary>
    /// Executed when a process has exited.
    /// </summary>
    event ProcessExited OnRemovedProcess;

    private static IProcessWatcher? _watcher;

    /// <summary>
    /// Returns a process watcher that appropriate for the given OS/program configuration.
    /// </summary>
    public static IProcessWatcher Get()
    {
        if (_watcher == null)
        {
            // On Wine (and modified Windows OS) WMI might not work.
            try { _watcher = Lib.IsElevated ? (IProcessWatcher)new WmiProcessWatcher() : ProcessWatcher.Instance; }
            catch (Exception) { _watcher = ProcessWatcher.Instance; }
        }

        return _watcher;
    }
}

/// <summary>
/// Called when a new process has been launched.
/// </summary>
/// <param name="newProcess">A process which has been recently launched.</param>
public delegate void ProcessArrived(Process newProcess);

/// <summary>
/// Called when a process has exited.
/// </summary>
/// <param name="processId">The process which has exited.</param>
public delegate void ProcessExited(int processId);