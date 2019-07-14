using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using Reloaded.Mod.Launcher.Misc;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Class that maintains an active collection of all processes that satisfy a given absolute file path.
    /// </summary>
    public class ApplicationInstanceTracker : IDisposable
    {
        public const int ProcessQueryLimitedInformation = 0x1000;
        public const int MaxPath = 32767;

        /// <summary>
        /// Fired whenever the list of processes satisfying the application path changes.
        /// </summary>
        public event ProcessesChanged OnProcessesChanged = processes => { }; 

        private HashSet<Process> _processes; // All processes that satisfy file path filter.
        private readonly string _applicationPath;
        private readonly ProcessWatcher _processWatcher;
        private readonly StringBuilder _buffer = new StringBuilder(MaxPath);

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
                    var processPath = Path.GetFullPath(GetExecutablePath(process.Id));

                    if (string.Equals(processPath, _applicationPath, StringComparison.OrdinalIgnoreCase))
                        _processes.Add(process);
                });
            }
        }

        /* Public API */

        public Structs.ProcessCollection GetProcesses()
        {
            var processCollection = Structs.ProcessCollection.GetEmpty(_processes.Count);
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
                if (module.ModuleName == Constants.LoaderDllName)
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

        /* Helpers */
        private string GetExecutablePath(int processId)
        {
            /* Vista+ implementation that is faster and allows to query system processes. */
            var processHandle   = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            
            // Note: We can re-use the buffer without clearing because the returned string is null-terminated.

            if (processHandle != IntPtr.Zero)
            {
                try
                {
                    // ReSharper disable once NotAccessedVariable
                    int size = _buffer.Capacity;
                    if (QueryFullProcessImageNameW(processHandle, 0, _buffer, out size))
                        return _buffer.ToString();
                }
                finally
                {
                    CloseHandle(processHandle);
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /* Definitions */
        public delegate void ProcessesChanged(Process[] newProcesses);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageNameW(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);
    }
}
