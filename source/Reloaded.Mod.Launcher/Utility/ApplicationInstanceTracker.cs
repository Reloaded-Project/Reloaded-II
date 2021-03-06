﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Utility.Interfaces;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Server;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Class that maintains an active collection of all processes that satisfy a given absolute file path.
    /// </summary>
    public class ApplicationInstanceTracker : IDisposable
    {
        /// <summary>
        /// Fired whenever the list of processes satisfying the application path changes.
        /// </summary>
        public event ProcessesChanged OnProcessesChanged = processes => { }; 

        private HashSet<Process> _processes; // All processes that satisfy file path filter.
        private readonly string _applicationPath;
        private readonly IProcessWatcher _processWatcher;

        /* Class Setup and Teardown */
        public ApplicationInstanceTracker(string applicationPath, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(applicationPath))
                throw new ArgumentException(Errors.PathNullOrEmpty());

            _applicationPath = Path.GetFullPath(applicationPath);
            BuildInitialProcesses(token);

            if (!token.IsCancellationRequested)
            {
                _processWatcher = IoC.Get<IProcessWatcher>();
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

                ActionWrappers.TryCatchDiscard(() =>
                {
                    var processPath = Path.GetFullPath(process.GetExecutablePath());

                    if (string.Equals(processPath, _applicationPath, StringComparison.OrdinalIgnoreCase))
                        _processes.Add(process);
                });
            }
        }

        /* Static API */

        /// <summary>
        /// Returns a complete list of all processes running Reloaded.
        /// </summary>
        /// <param name="processes">List of all processes running Reloaded.</param>
        /// <returns>True if there is more than one process, else false.</returns>
        public static bool GetAllProcesses(out IEnumerable<Process> processes)
        {
            var applications        = ApplicationConfig.GetAllApplications();
            var trackers            = applications.Select(x => new ApplicationInstanceTracker(x.Config.AppLocation));
            processes               = trackers.SelectMany(x => x.GetProcesses().ReloadedProcesses);

            return processes.Any();
        }

        /* Public API */
        public Structs.ProcessCollection GetProcesses()
        {
            var processCollection = Structs.ProcessCollection.GetEmpty(_processes.Count);
            foreach (var process in _processes.ToArray())
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
            try
            {
                Client.GetPort(process.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /* Active maintenance of list based off of process add/remove events. */
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

        private void AddProcess(Process newProcess)
        {
            if (string.Equals(ProcessExtensions.GetExecutablePath(newProcess), _applicationPath, StringComparison.InvariantCultureIgnoreCase))
            {
                _processes.Add(newProcess);
                RaiseOnProcessesChanged();
            }
        }

        private void ProcessWatcherOnOnRemovedProcess(int processid)  => RemoveProcess(processid);
        private void ProcessWatcherOnOnNewProcess(Process newprocess) => AddProcess(newprocess);

        /* Definitions */
        public delegate void ProcessesChanged(Process[] newProcesses);
    }
}
