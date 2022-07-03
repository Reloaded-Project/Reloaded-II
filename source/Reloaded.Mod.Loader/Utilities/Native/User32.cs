namespace Reloaded.Mod.Loader.Utilities.Native;

public static class User32
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(int hWnd, String text, String caption, uint type);
}