using System;
using System.Diagnostics;
using System.Management;
using Reloaded.Mod.Launcher.Utility.Interfaces;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Utility 
{
    /// <summary>
    /// Utility class that provides events for when processes start up and/or shut down using WMI.
    /// </summary>
    public class WmiProcessWatcher : ObservableObject, IDisposable, IProcessWatcher
    {
        private const string WmiProcessidName = "ProcessID";

        public event ProcessArrived OnNewProcess     = process   => { };
        public event ProcessExited  OnRemovedProcess = processId => { };

        private readonly ManagementEventWatcher _startWatcher;
        private readonly ManagementEventWatcher _stopWatcher;

        public WmiProcessWatcher()
        {
            // Populate bindings.
            _startWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _stopWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _startWatcher.EventArrived += ApplicationLaunched;
            _stopWatcher.EventArrived += ApplicationExited;
            _startWatcher.Start();
            _stopWatcher.Start();
        }

        ~WmiProcessWatcher()
        {
            Dispose();
        }

        public void Dispose()
        {
            _startWatcher?.Dispose();
            _stopWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected virtual void RaiseOnNewProcess(Process process)
        {
            OnNewProcess(process);
        }

        protected virtual void RaiseOnRemovedProcess(int processId)
        {
            OnRemovedProcess(processId);
        }

        private void ApplicationLaunched(object sender, EventArrivedEventArgs e)
        {
            ActionWrappers.TryCatch(() =>
            {
                var processId = Convert.ToInt32(e.NewEvent.Properties[WmiProcessidName].Value);
                var process = Process.GetProcessById(processId);
                RaiseOnNewProcess(process);
            });
        }

        private void ApplicationExited(object sender, EventArrivedEventArgs e)
        {
            int processId = Convert.ToInt32(e.NewEvent.Properties[WmiProcessidName].Value);
            RaiseOnRemovedProcess(processId);
        }
    }
}
