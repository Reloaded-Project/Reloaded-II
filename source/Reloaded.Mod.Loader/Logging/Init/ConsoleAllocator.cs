using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Loader.Logging.Init;

public static class ConsoleAllocator
{
    /// <summary>
    /// Returns true if a console window is present for this process, else false.
    /// </summary>
    public static bool ConsoleExists => GetConsoleWindow() != IntPtr.Zero;

    /// <summary>
    /// Creates a new console instance if no console is present for the console.
    /// </summary>
    public static bool Alloc()
    {
        // Hack for Wine: In Wine, GetConsoleWindow is stubbed and thus cannot evaluate properly.
        if (Environment.IsWine)
        {
            AllocConsole();
            return true;
        }

        if (!ConsoleExists)
            return AllocConsole();

        return true;
    }

    /// <summary>
    /// If the process has an associated console, it will be detached and no longer visible.
    /// </summary>
    public static bool Free()
    {
        // Hack for Wine: In Wine, GetConsoleWindow is stubbed and thus cannot evaluate properly.
        if (Environment.IsWine)
        {
            FreeConsole();
            return true;
        }

        if (ConsoleExists)
            return FreeConsole();

        return false;
    }

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
}