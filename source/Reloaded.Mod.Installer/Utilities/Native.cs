namespace Reloaded.Mod.Installer.Utilities;

public static class Native
{
    [DllImport("Shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", CharSet = CharSet.Unicode)]
    public static extern bool IsDirectoryEmpty(string directory);
}