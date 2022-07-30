namespace Reloaded.Mod.Shared;

public static class Environment
{
    /// <summary>
    /// True if executing under Wine, else false.
    /// </summary>
    public static bool IsWine { get; }

    /// <summary>
    /// Gets the full path to the current process.
    /// </summary>
    public static Process CurrentProcess { get; } = Process.GetCurrentProcess();

    /// <summary>
    /// Gets the full path to the current process.
    /// </summary>
    public static Lazy<string> CurrentProcessLocation { get; } = new(() => Path.GetFullPath(CurrentProcess.MainModule.FileName));

    static Environment()
    {
        var ntdll = GetModuleHandle("ntdll.dll");
        IsWine    = GetProcAddress(ntdll, "wine_get_version") != IntPtr.Zero;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}