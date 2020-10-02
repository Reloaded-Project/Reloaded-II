namespace Reloaded.Mod.Loader.Utilities
{
    /// <summary>
    /// Adds relevant text to log messages.
    /// </summary>
    public static class LogMessageFormatter
    {
        public static string AddLogPrefix(string message, string prefix = "[Reloaded] ") => prefix + message;
    }
}
