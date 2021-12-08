using System;
using System.Diagnostics;

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
            Actions.SynchronizationContext.Send((x) => Actions.DisplayMessagebox?.Invoke(Resources.ErrorUnknown.Get(), $"{message}{ex.Message}\n{ex.StackTrace}"), null);
        else
            Debugger.Break();
    }
}