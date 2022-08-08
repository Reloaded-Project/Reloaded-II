namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility class that provides events for when processes start up and/or shut down using WMI.
/// </summary>
public class WmiProcessWatcher : ObservableObject, IProcessWatcher
{
    private const string WmiProcessidName = "ProcessID";

    /// <inheritdoc />
    public event ProcessArrived OnNewProcess     = _   => { };

    /// <inheritdoc />
    public event ProcessExited  OnRemovedProcess = _ => { };

    private readonly ManagementEventWatcher _startWatcher;
    private readonly ManagementEventWatcher _stopWatcher;

    /// <inheritdoc />
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

    /// <inheritdoc />
    ~WmiProcessWatcher() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _startWatcher.EventArrived -= ApplicationLaunched;
        _stopWatcher.EventArrived -= ApplicationExited;
        _startWatcher.Stop();
        _stopWatcher.Stop();
        _startWatcher.Dispose();
        _stopWatcher.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private void ApplicationLaunched(object sender, EventArrivedEventArgs e)
    {
        ActionWrappers.TryCatchDiscard(() =>
        {
            var processId = Convert.ToInt32(e.NewEvent.Properties[WmiProcessidName].Value);
            var process = Process.GetProcessById(processId);
            OnNewProcess(process);
        });
    }

    private void ApplicationExited(object sender, EventArrivedEventArgs e)
    {
        int processId = Convert.ToInt32(e.NewEvent.Properties[WmiProcessidName].Value);
        OnRemovedProcess(processId);
    }
}