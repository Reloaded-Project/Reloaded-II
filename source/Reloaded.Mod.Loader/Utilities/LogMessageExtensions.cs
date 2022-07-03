namespace Reloaded.Mod.Loader.Utilities;

/// <summary>
/// Adds relevant text to log messages.
/// </summary>
public static class LogMessageExtensions
{
    public static void LogWriteLine(this ILogger logger, string message, Color color) => logger.WriteLine(AddLogPrefix(message), color);
    public static void LogWriteLineAsync(this ILogger logger, string message) => logger.WriteLineAsync(AddLogPrefix(message));
    public static void LogWriteLineAsync(this ILogger logger, string message, Color color) => logger.WriteLineAsync(AddLogPrefix(message), color);

    public static string AddLogPrefix(string message, string prefix = "[Reloaded] ") => prefix + message;
}