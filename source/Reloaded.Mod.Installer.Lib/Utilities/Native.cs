namespace Reloaded.Mod.Installer.Lib.Utilities;

public static class Native
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}