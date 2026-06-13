using System.Linq;
namespace Reloaded.Mod.Installer.Lib;

/// <summary>
///     Settings for the installer.
/// </summary>
public class Settings
{
    public string InstallLocation { get; set; } = Path.Combine(GetSafeInstallPath(), "Reloaded-II");
    public bool IsManuallyOverwrittenLocation { get; set; }
    public bool CreateShortcut { get; set; } = true;
    public bool HideNonErrorGuiMessages { get; set; } = false;
    public bool StartReloaded { get; set; } = true;
    
    public Settings() { }
    
    public static Settings GetSettings(string[] args)
    {
        var settings = new Settings();
        for (int x = 0; x < args.Length - 1; x++)
        {
            if (args[x] == "--installdir")
            {
                settings.InstallLocation = args[x + 1];
                settings.IsManuallyOverwrittenLocation = true;
            }
            if (args[x] == "--nogui") settings.HideNonErrorGuiMessages = true;
            if (args[x] == "--nocreateshortcut") settings.CreateShortcut = false;
            if (args[x] == "--nostartreloaded") settings.StartReloaded = false;
        }

        return settings;
    }

    private static string GetSafeInstallPath()
    {
        var installPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (IsPathInCloudSyncFolder(installPath))
        {
            var driveRoot = Path.GetPathRoot(Environment.SystemDirectory);
            if (driveRoot == null)
                // if for some reason we can't determine the root, fallback to Desktop
                return installPath;
            return driveRoot;
        }
        return installPath;
    }

    /// <summary>
    /// Checks whether a given path is inside a cloud sync folder (OneDrive, Dropbox, Google Drive,
    /// iCloud, Box, MEGA, etc.). Reloaded installed into such folders is avoided because many mods
    /// do not tolerate cloud offload/locking, and load times are poor.
    ///
    /// Detection is layered, checked in order:
    ///
    /// - OneDrive environment variables (<c>OneDrive</c> / <c>OneDriveCommercial</c>).
    ///
    /// - The Windows Cloud Files API (<c>CfGetSyncRootInfoByPath</c>, available on Windows 10 1709+),
    ///   which detects any provider that registers a sync root.
    ///
    /// Desktop is only ever redirected by OneDrive, so these two tiers are sufficient for the install-path check.
    /// </summary>
    private static bool IsPathInCloudSyncFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string fullPath;
        try { fullPath = Path.GetFullPath(path); }
        catch { return false; }

        return IsInOneDrive(fullPath)
            || IsInRegisteredCloudSyncRoot(fullPath);
    }

    // --- Tier 1: OneDrive environment variables ---
    private static bool IsInOneDrive(string fullPath)
    {
        foreach (var envVar in s_oneDriveEnvVars)
        {
            var root = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(root))
                continue;

            try
            {
                var fullRoot = Path.GetFullPath(root)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (IsUnder(fullPath, fullRoot))
                    return true;
            }
            catch { /* malformed path/env - skip */ }
        }

        return false;
    }

    // --- Tier 2: Cloud Files API (cldapi.dll), Windows 10 1709+ ---
    // A single native call reports whether the path is inside any registered cloud sync root,
    // regardless of provider. No ancestor walk or hydration-state dependence.
    private static bool IsInRegisteredCloudSyncRoot(string fullPath)
    {
        try
        {
            // CF_SYNC_ROOT_INFO_CLASS.CfSyncRootInfoBasic == 0.
            // A small buffer is plenty for the basic info struct; we only care about the HRESULT.
            var buffer = new byte[256];
            int hr = CfGetSyncRootInfoByPath(fullPath, 0, buffer, buffer.Length, out _);
            return hr >= 0; // S_OK (0) => the path resolves to a registered sync root.
        }
        catch
        {
            // cldapi.dll missing (older than Windows 10 1709) or call failed: rely on other tiers.
            return false;
        }
    }

    /// <summary>
    /// True if <paramref name="fullPath"/> equals or is a descendant of <paramref name="root"/>.
    /// Case-insensitive. The separator is appended on the prefix check so a root named
    /// e.g. "OneDrive" does not match a sibling like "OneDriveBackup".
    /// </summary>
    private static bool IsUnder(string fullPath, string root)
    {
        var trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (trimmedRoot.Length == 0)
            return false;

        return fullPath.Equals(trimmedRoot, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(trimmedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    [DllImport("cldapi.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static extern int CfGetSyncRootInfoByPath(
        string filePath,
        int infoClass,
        [Out] byte[] infoBuffer,
        int infoBufferSize,
        out int returnedLength);

    private static readonly string[] s_oneDriveEnvVars = { "OneDrive", "OneDriveCommercial" };
}