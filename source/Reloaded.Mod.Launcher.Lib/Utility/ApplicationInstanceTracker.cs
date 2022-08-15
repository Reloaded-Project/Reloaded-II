namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Class that maintains an active collection of all processes that satisfy a given absolute file path.
/// </summary>
public class ApplicationInstanceTracker : IDisposable
{
    /// <summary>
    /// Fired whenever the list of processes satisfying the application path changes.
    /// </summary>
    public event ProcessesChanged OnProcessesChanged = processes => { }; 

    private HashSet<Process>? _processes; // All processes that satisfy file path filter.
    private readonly string _applicationPath;
    private readonly IProcessWatcher? _processWatcher;

    /* Class Setup and Teardown */
    
    /// <summary/>
    public ApplicationInstanceTracker(string applicationPath, CancellationToken token = default)
    {
        if (String.IsNullOrEmpty(applicationPath))
            throw new ArgumentException(Resources.ErrorPathNullOrEmpty.Get());

        _applicationPath = Path.GetFullPath(applicationPath);
        BuildInitialProcesses(token);

        if (token.IsCancellationRequested) 
            return;

        _processWatcher = IoC.GetConstant<IProcessWatcher>();
        _processWatcher.OnNewProcess += ProcessWatcherOnOnNewProcess;
        _processWatcher.OnRemovedProcess += ProcessWatcherOnOnRemovedProcess;
    }

    /// <inheritdoc />
    ~ApplicationInstanceTracker() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_processWatcher != null)
        {
            _processWatcher.OnNewProcess -= ProcessWatcherOnOnNewProcess;
            _processWatcher.OnRemovedProcess -= ProcessWatcherOnOnRemovedProcess;
        }

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
        var trackers            = applications.Select(x => new ApplicationInstanceTracker(ApplicationConfig.GetAbsoluteAppLocation(x)));
        processes               = trackers.SelectMany(x => x.GetProcesses().ReloadedProcesses);

        return processes.Any();
    }

    /* Public API */

    /// <summary>
    /// Returns a collection of all Reloaded and non-Reloaded processes on the local machine.
    /// </summary>
    /// <returns>List, incomplete if cancellation requested.</returns>
    public ProcessCollection GetProcesses(CancellationToken token = default)
    {
        if (_processes == null)
            return default;

        var processCollection = ProcessCollection.GetEmpty(_processes.Count);
        foreach (var process in _processes.ToArray())
        {
            // Return if cancellation requested.
            if (token.IsCancellationRequested)
                return processCollection;

            if (ReloadedMappedFile.Exists(process.Id))
                processCollection.ReloadedProcesses.Add(process);
            else
                processCollection.NonReloadedProcesses.Add(process);
        }

        return processCollection;
    }

    /* Active maintenance of list based off of process add/remove events. */
    private void RaiseOnProcessesChanged() => OnProcessesChanged(_processes?.ToArray());

    private void RemoveProcess(int processId)
    {
        if (_processes == null)
            return;

        var process = _processes.FirstOrDefault(x => x.Id == processId);
        if (process == null)
            return;

        _processes.Remove(process);

        RaiseOnProcessesChanged();
    }

    private void AddProcess(Process newProcess)
    {
        if (_processes == null)
            return;

        if (!string.Equals(newProcess.GetExecutablePath(), _applicationPath, StringComparison.InvariantCultureIgnoreCase)) 
            return;

        _processes.Add(newProcess);
        RaiseOnProcessesChanged();
    }

    private void ProcessWatcherOnOnRemovedProcess(int processId)  => RemoveProcess(processId);

    private void ProcessWatcherOnOnNewProcess(Process newProcess) => AddProcess(newProcess);
}

/// <summary>
/// Executed when the list of running processes on the system changes.
/// </summary>
public delegate void ProcessesChanged(Process[]? newProcesses);

/// <summary>
/// Stores a list of Reloaded and non-Reloaded processes.
/// </summary>
public struct ProcessCollection
{
    /// <summary>
    /// List of processes where Reloaded is loaded.
    /// </summary>
    public List<Process> ReloadedProcesses;

    /// <summary>
    /// List of processes where Reloaded is not loaded.
    /// </summary>
    public List<Process> NonReloadedProcesses;

    /// <summary/>
    public ProcessCollection(List<Process> reloadedProcesses, List<Process> nonReloadedProcesses)
    {
        ReloadedProcesses = reloadedProcesses;
        NonReloadedProcesses = nonReloadedProcesses;
    }

    /// <summary>
    /// Returns an empty collection of processes.
    /// </summary>
    /// <param name="maxSize">Size of the underlying containers.</param>
    public static ProcessCollection GetEmpty(int maxSize = 0)
    {
        return new ProcessCollection(new List<Process>(maxSize), new List<Process>(maxSize));
    }
}