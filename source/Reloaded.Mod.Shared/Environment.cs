namespace Reloaded.Mod.Shared;

/// <summary>
/// Provides various pieces of information about the currently operating environment.
/// </summary>
public static class Environment
{
    /// <summary>
    /// True if executing under Wine, else false.
    /// </summary>
    public static bool IsWine { get; }
    
    /// <summary>
    /// True if executing under Protontricks, else false.
    /// </summary>
    public static bool IsProtontricks { get; }
    
    /// <summary>
    /// Determines if the game launch dialog needs to be shown before launching a game.
    /// This is enabled for Wine and disabled for Protontricks.
    /// </summary>
    public static bool RequiresWineLaunchDialog { get; }

    /// <summary>
    /// Gets the full path to the current process.
    /// </summary>
    public static Process CurrentProcess { get; } = Process.GetCurrentProcess();

    /// <summary>
    /// Gets the full path to the current process.
    /// </summary>
    public static Lazy<string> CurrentProcessLocation { get; } = new(() => Path.GetFullPath(CurrentProcess.MainModule!.FileName!));

    static Environment()
    {
        var ntdll = GetModuleHandle("ntdll.dll");
        IsWine = GetProcAddress(ntdll, "wine_get_version") != IntPtr.Zero;
        IsProtontricks = !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("STEAM_APPID"));
        RequiresWineLaunchDialog = IsWine && !IsProtontricks;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}