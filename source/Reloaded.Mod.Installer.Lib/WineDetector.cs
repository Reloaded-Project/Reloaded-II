namespace Reloaded.Mod.Installer.Lib;

internal class WineDetector
{
    internal static bool IsWine()
    {
        var ntdll = GetModuleHandle("ntdll.dll");
        return GetProcAddress(ntdll, "wine_get_version") != IntPtr.Zero;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}