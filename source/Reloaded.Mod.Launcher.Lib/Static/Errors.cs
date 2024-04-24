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
                    Actions.DisplayMessagebox.Invoke(Resources.ErrorUnknown.Get(), $"{message}{ex.Message}\n{ex.StackTrace}");
                }
                else
                {
                    MessageBox.Show($"{message}{ex.Message}\n{ex.StackTrace}", Resources.ErrorUnknown.Get());
                }
            }
        }
        else
            Debugger.Break();
    }
}