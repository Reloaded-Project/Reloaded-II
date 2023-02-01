namespace Reloaded.Mod.Loader.IO.Utility;

public static class Extensions
{
    /// <summary>
    /// Posts the event to the synchronization context if it is available, else directly executes it.
    /// </summary>
    public static void Post(this SynchronizationContext context, Action action)
    {
        if (context != null)
            context.Post(state => action(), null);
        else
            action();
    }
}