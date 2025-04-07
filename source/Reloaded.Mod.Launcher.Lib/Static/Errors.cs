using System.Windows;
using static System.Environment;

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
        if (!Debugger.IsAttached)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            var isUiInitialized = Actions.SynchronizationContext != null && Actions.DisplayMessagebox != null;
            if (isUiInitialized)
            {
                // Just in case of an error before proper UI init.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (Actions.DisplayMessagebox != null)
                {
                    var logPath = $"{Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "Reloaded-Mod-Loader-II", "ApplicationLogs", DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss"))}.txt";
                    bool result = Actions.DisplayMessagebox.Invoke(Resources.ErrorUnknown.Get(), $"{message}{ex.Message}\nDo you wish to view the stacktrace for more information?", new Actions.DisplayMessageBoxParams
                    {
                        Type = Actions.MessageBoxType.OkCancel
                    });
                    if (result)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                        File.WriteAllText(logPath, $"Exception:\n{ex.Message}\nStacktrace:\n{ex.StackTrace}");
                        ProcessStartInfo logFile = new()
                        {
                            FileName = logPath,
                            UseShellExecute = true
                        };
                        Process.Start(logFile);
                    }
                }
                else
                {
                    var logPath = $"{Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "Reloaded-Mod-Loader-II", "ApplicationLogs", DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss"))}.txt";
                    var result = MessageBox.Show($"{message}{ex.Message}\nDo you wish to view the stacktrace for more information?", Resources.ErrorUnknown.Get(), MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                        File.WriteAllText(logPath, $"Exception:\n{ex.Message}\nStacktrace:\n{ex.StackTrace}");
                        ProcessStartInfo logFile = new()
                        {
                            FileName = logPath,
                            UseShellExecute = true
                        };
                        Process.Start(logFile);
                    }
                }
            }
        }
        else
            Debugger.Break();
    }
}