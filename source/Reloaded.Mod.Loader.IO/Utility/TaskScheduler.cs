namespace Reloaded.Mod.Loader.IO.Utility;

/// <summary>
/// A utility class that allows for the scheduling of a task (<see cref="Action"/>)
/// to be executed after a certain length of time.
/// </summary>
public class TaskScheduler : IDisposable
{
    /// <summary>
    /// The current action to execute.
    /// </summary>
    private Action<CancellationToken> _actionToExecute;

    /// <summary>
    /// The timer used to execute the ticks.
    /// </summary>
    private Timer _timer;

    /// <summary>
    /// The token source for the token used to cancel the executed events.
    /// </summary>
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();

    private TimeSpan _currentTickDuration;

    /// <summary>
    /// Creates an <see cref="TaskScheduler"/> given the length of time between ticks.
    /// </summary>
    /// <param name="delayMilliseconds">Amount of milliseconds before a scheduled action is ran.</param>
    public TaskScheduler(int delayMilliseconds)
    {
        _currentTickDuration = TimeSpan.FromMilliseconds(delayMilliseconds);
        _timer = new Timer(Tick, null, TimeSpan.Zero, _currentTickDuration);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Cancels the currently scheduled task and schedules a new task.
    /// </summary>
    public void Schedule(Action<CancellationToken> action)
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource();

        _timer.Change(_currentTickDuration, _currentTickDuration);
        _actionToExecute = action;
    }

    /// <summary>
    /// Sets the time taken before the next scheduled task is ran.
    /// </summary>
    public void SetSchedule(TimeSpan timeSpan)
    {
        _currentTickDuration = timeSpan;
        _timer.Change(TimeSpan.Zero, timeSpan);
    }

    /* Tick Code */
    private void Tick(object state)
    {
        if (_actionToExecute == null)
            return;

        _actionToExecute.Invoke(_tokenSource.Token);
        _actionToExecute = null;
    }
}