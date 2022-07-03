namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility class that provides extensions for <see cref="Action{T}"/> and <see cref="Func{TResult}"/>.
/// </summary>
public static class ActionWrappers
{
    /// <summary>
    /// A wrapper for try/catch that displays exceptions in the UI.
    /// </summary>
    public static void TryCatch(Action action)
    {
        try { action(); }
        catch (Exception ex) { Errors.HandleException(ex); }
    }

    /// <summary>
    /// A wrapper for try/catch that swallows exceptions.
    /// </summary>
    public static void TryCatchDiscard(Action action)
    {
        try { action(); }
        catch (Exception) { /* ignored */ }
    }

    /// <summary>
    /// Executes an action on the UI thread, allowing for collections that run on it to be manipulated.
    /// </summary>
    public static void ExecuteWithApplicationDispatcher(Action action)
    {
        Actions.SynchronizationContext.Send(_ => action(), null);
    }

    /// <summary>
    /// Executes an action on the UI thread asynchronously, allowing for
    /// collections that run on it to be manipulated.
    /// </summary>
    public static void ExecuteWithApplicationDispatcherAsync(Action action)
    {
        Actions.SynchronizationContext.Post(_ => action(), null);
    }
    
    /// <summary>
    /// Executes an action on the UI thread asynchronously, allowing for
    /// collections that run on it to be manipulated.
    /// </summary>
    public static async Task<T> ExecuteWithApplicationDispatcherAsync<T>(Func<T> function)
    {
        T? result = default;
        Exception? exception = null;

        Actions.SynchronizationContext.Post(_ =>
        {
            try { result = function(); }
            catch (Exception? ex) { exception = ex; }
        }, null);
        
        // Wait until result is acquired.
        while (result == null && exception == null)
            await Task.Delay(16);

        // Throw exception if task faulted.
        if (exception != null)
            throw exception;
        
        return result!;
    }

    /// <param name="condition">Stops sleeping if this condition returns true.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    public static void SleepOnConditionWithTimeout(Func<bool> condition, int timeout, int sleepTime)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        while (watch.ElapsedMilliseconds < timeout && !condition())
        {
            Thread.Sleep(sleepTime);                
        }
    }

    /// <param name="getValue">Function that retrieves the value.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    /// <param name="token">Token that allows for cancellation of the task.</param>
    /// <exception cref="Exception">Timeout expired.</exception>
    public static T? TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default)
    {
        var watch = new Stopwatch();
        watch.Start();
        bool valueSet = false;
        T? value = default;

        while (watch.ElapsedMilliseconds < timeout)
        {
            if (token.IsCancellationRequested)
                return value;

            try
            {
                value = getValue();
                valueSet = true;
                break;
            }
            catch (Exception) { /* Ignored */ }

            Thread.Sleep(sleepTime);
        }

        if (valueSet == false)
            throw new Exception($"Timeout limit {timeout} exceeded.");

        return value;
    }

    /// <param name="getValue">Function that retrieves the value.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    /// <param name="token">Token that allows for cancellation of the task.</param>
    /// <exception cref="Exception">Timeout expired.</exception>
    public static async Task<T?> TryGetValueAsync<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default)
    {
        var watch = new Stopwatch();
        watch.Start();
        bool valueSet = false;
        T? value = default;

        while (watch.ElapsedMilliseconds < timeout)
        {
            if (token.IsCancellationRequested)
                return value;

            try
            {
                value = getValue();
                valueSet = true;
                break;
            }
            catch (Exception) { /* Ignored */ }

            await Task.Delay(sleepTime, token);
        }

        if (valueSet == false)
            throw new Exception($"Timeout limit {timeout} exceeded.");

        return value;
    }

    /// <summary>
    /// Attempts to obtain a value while either the timeout has not expired or the <paramref name="whileFunction"/> returns
    /// true.
    /// </summary>
    /// <param name="getValue">Function that retrieves the value.</param>
    /// <param name="whileFunction">Keep trying while this condition is true.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    /// <param name="token">Token that allows for cancellation of the task.</param>
    /// <exception cref="Exception">Timeout expired.</exception>
    public static T? TryGetValueWhile<T>(Func<T> getValue, Func<bool> whileFunction, int timeout, int sleepTime, CancellationToken token = default)
    {
        var watch = new Stopwatch();
        watch.Start();
        bool valueSet = false;
        T? value = default;

        while (watch.ElapsedMilliseconds < timeout || whileFunction())
        {
            if (token.IsCancellationRequested)
                return value;

            try
            {
                value = getValue();
                valueSet = true;
                break;
            }
            catch (Exception) { /* Ignored */ }

            Thread.Sleep(sleepTime);
        }

        if (valueSet == false)
            throw new Exception($"Timeout limit {timeout} exceeded.");

        return value;
    }
}