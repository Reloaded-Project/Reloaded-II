using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Reloaded.Mod.Launcher.Utility.Interfaces;
using Reloaded.Mod.Loader.IO.Weaving;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Utility class that provides events for when processes start up and/or shut down without using WMI.
    /// More resource wasteful and has higher latency but does not require administrative privileges.
    /// </summary>
    public class ProcessWatcher : ObservableObject, IDisposable, IProcessWatcher
    {
        public static ProcessWatcher Instance { get; } = new ProcessWatcher();
        private static XamlResource<int> _refreshInterval { get; set; }

        public event ProcessArrived OnNewProcess    = process => { };
        public event ProcessExited OnRemovedProcess = processId => { };
        private Timer _timer;
        private ObservableCollection<int> _processes;
        private object _lock = new object();


        public ProcessWatcher()
        {
            if (_refreshInterval == null)
                _refreshInterval = new XamlResource<int>("ReloadedProcessListRefreshInterval");

            _processes  = new ObservableCollection<int>(Shared.ProcessExtensions.GetProcessIds());
            _processes.CollectionChanged += ProcessesChanged;
            _timer = new Timer(Tick, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(_refreshInterval.Get()));
        }

        private void Tick(object state)
        {
            lock (_lock)
            {
                var processIds = Shared.ProcessExtensions.GetProcessIds();
                Collections.ModifyObservableCollection(_processes, processIds);
            }
        }

        private void ProcessesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (var newItem in e.NewItems)
                    {
                        try { OnNewProcess(Process.GetProcessById((int)newItem)); }
                        catch (Exception) { }
                    }
                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (var oldItem in e.OldItems)
                        OnRemovedProcess((int) oldItem);
                    break;
                }
            }
        }

        public void Dispose() { }
    }
}
