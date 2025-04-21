using System.Windows;

namespace Reloaded.Mod.Launcher.Lib.Static;

/// <summary>
/// Utilities for easier exception handling.
/// </summary>
public static class Errors
{
    /// <summary>
    /// Handles a generic thrown exception.
    /// </summary>
    public static void HandleException(Exception ex, string message = "")
    {
        if (Debugger.IsAttached)
            Debugger.Break();
        else
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            var canDisplayMessageBox = Actions.DisplayMessagebox != null;
            var hasSynchronizationContext = Actions.SynchronizationContext != null;
            var isUiInitialized = hasSynchronizationContext && canDisplayMessageBox;
            // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (!isUiInitialized) return;

            var errorMessage = $"{message}{ex.Message}\n\n{Resources.ErrorViewDetails.Get()}";
            bool userWantsToSeeStackTrace;

            // Just in case of an error before proper UI init.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (canDisplayMessageBox)
            {
                userWantsToSeeStackTrace = Actions.DisplayMessagebox!.Invoke(
                    Resources.ErrorUnknown.Get(),
                    errorMessage,
                    new Actions.DisplayMessageBoxParams
                    {
                        Type = Actions.MessageBoxType.OkCancel
                    });
            }
            else
            {
                var result = MessageBox.Show(
                    errorMessage,
                    Resources.ErrorUnknown.Get(), 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Error);
                
                userWantsToSeeStackTrace = (result == MessageBoxResult.Yes);
            }

            if (userWantsToSeeStackTrace)
                CreateAndOpenLogFile(ex, Path.Combine(Paths.LauncherErrorsPath, $"{DateTime.UtcNow:yyyy-MM-dd HH.mm.ss}.txt"));
        }
    }

    /// <summary>
    /// Creates and opens a log file with exception details.
    /// </summary>
    private static void CreateAndOpenLogFile(Exception ex, string logPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        var text = $"{Resources.ErrorStacktraceTitle.Get()}\n" +
                   $"{Resources.ErrorStacktraceSubtitle.Get()}\n" +
                   $"-------------\n" +
                   $"Exception:\n" +
                   $"{ex.Message}\n" +
                   $"Stacktrace:\n" +
                   $"{ex.StackTrace}";
        File.WriteAllText(logPath, text);
        
        ProcessStartInfo logFile = new()
        {
            FileName = logPath,
            UseShellExecute = true
        };
        Process.Start(logFile);
    }
}