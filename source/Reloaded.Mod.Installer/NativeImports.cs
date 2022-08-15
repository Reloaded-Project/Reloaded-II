namespace Reloaded.Mod.Installer;

internal class NativeImports
{
    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    internal const int SW_HIDE = 0;
}