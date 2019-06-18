using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Class that maintains an active collection of all processes that satisfy a given absolute file path.
    /// </summary>
    public class ApplicationInstanceTracker : IDisposable
    {
        public const string LoaderDllName = "Reloaded.Mod.Loader.dll";

        /// <summary>
        /// Fired whenever the list of processes satisfying the application path changes.
        /// </summary>
        public event ProcessesChanged OnProcessesChanged = processes => { }; 

        private HashSet<Process> _processes; // All processes that satisfy file path filter.
        private string _applicationPath;
        private ProcessWatcher _processWatcher;

        /* Class Setup and Teardown */

        public ApplicationInstanceTracker(string applicationPath, CancellationToken token = default)
        {
            _applicationPath = Path.GetFullPath(applicationPath);
            BuildInitialProcesses(token);

            if (!token.IsCancellationRequested)
            {
                _processWatcher = new ProcessWatcher();
                _processWatcher.OnNewProcess += ProcessWatcherOnOnNewProcess;
                _processWatcher.OnRemovedProcess += ProcessWatcherOnOnRemovedProcess;
            }
        }

        ~ApplicationInstanceTracker()
        {
            Dispose();
        }

        public void Dispose()
        {
            _processWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void BuildInitialProcesses(CancellationToken token = default)
        {
            var allProcesses = Process.GetProcesses();
            _processes = new HashSet<Process>(allProcesses.Length);

            foreach (var process in allProcesses)
            {
                if (token.IsCancellationRequested)
                    return;

                ActionWrappers.TryCatch(() =>
                {
                    var processPath = Path.GetFullPath(process.MainModule.FileName);

                    if (string.Equals(processPath, _applicationPath, StringComparison.OrdinalIgnoreCase))
                        _processes.Add(process);
                });
            }
        }

        /* Public API */

        public ProcessCollection GetProcesses()
        {
            var processCollection = ProcessCollection.GetEmpty(_processes.Count);
            foreach (var process in _processes)
            {
                if (IsModLoaderPresent(process))
                    processCollection.ReloadedProcesses.Add(process);
                else
                    processCollection.NonReloadedProcesses.Add(process);
            }

            return processCollection;
        }

        private bool IsModLoaderPresent(Process process)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == LoaderDllName)
                {
                    return true;
                }
            }
            return false;
        }

        /* This section: Active maintenance of list based off of process add/remove events. */
        private void RaiseOnProcessesChanged()
        {
            OnProcessesChanged(_processes.ToArray());
        }

        private void RemoveProcess(int processId)
        {
            var process = _processes.FirstOrDefault(x => x.Id == processId);
            if (process != null)
            {
                _processes.Remove(process);
                RaiseOnProcessesChanged();
            }
        }

        private void AddProcess(Process newprocess)
        {
            if (string.Equals(Path.GetFullPath(newprocess.MainModule.FileName), _applicationPath,
                StringComparison.InvariantCultureIgnoreCase))
            {
                _processes.Add(newprocess);
                RaiseOnProcessesChanged();
            }
        }

        private void ProcessWatcherOnOnRemovedProcess(int processid)  => RemoveProcess(processid);
        private void ProcessWatcherOnOnNewProcess(Process newprocess) => AddProcess(newprocess);

        /* Definitions */
        public delegate void ProcessesChanged(Process[] newProcesses);
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
}
