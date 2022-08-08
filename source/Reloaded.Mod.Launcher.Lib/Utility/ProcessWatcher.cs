namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility class that provides events for when processes start up and/or shut down without using WMI.
/// More resource wasteful and has higher latency but does not require administrative privileges.
/// </summary>
public class ProcessWatcher : ObservableObject, IProcessWatcher
{
    /// <summary>
    /// Singleton instance of this class.
    /// </summary>
    public static ProcessWatcher Instance { get; } = new ProcessWatcher();

    /// <inheritdoc />
    public event ProcessArrived OnNewProcess    = _ => { };

    /// <inheritdoc />
    public event ProcessExited OnRemovedProcess = _ => { };
    
    private Timer _timer;
    private readonly ObservableCollection<int> _processes;
    private object _lock = new object();

    /// <inheritdoc />
    public ProcessWatcher()
    {
        var interval = IoC.Get<LoaderConfig>().ReloadedProcessListRefreshInterval;
        _processes   = new ObservableCollection<int>(ProcessExtensions.GetProcessIds());
        _processes.CollectionChanged += ProcessesChanged;
        _timer = new Timer(Tick, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(interval));
    }

    /// <inheritdoc />
    public void Dispose() => _timer.Dispose();

    private void Tick(object? state)
    {
        lock (_lock)
        {
            var processIds = ProcessExtensions.GetProcessIds();
            Collections.ModifyObservableCollection(_processes, processIds);
        }
    }

    private void ProcessesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
            {
                if (e.NewItems == null)
                    return;

                foreach (var newItem in e.NewItems)
                {
                    try { OnNewProcess(Process.GetProcessById((int)newItem)); }
                    catch (Exception) { /* Ignored */ }
                }
                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                if (e.OldItems == null)
                    return;

                foreach (var oldItem in e.OldItems)
                    OnRemovedProcess((int) oldItem);

                break;
            }
        }
    }
}