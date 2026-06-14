using System.Runtime.InteropServices;
using Environment = System.Environment;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility methods for inspecting file system paths.
/// </summary>
public static class PathUtility
{
    /// <summary>
    /// Checks whether a given path is inside a cloud sync folder (OneDrive, Dropbox, Google Drive,
    /// iCloud, Box, MEGA, etc.). Reloaded and games inside such folders are avoided because many mods
    /// do not tolerate cloud offload/locking, and load times are poor.
    ///
    /// Detection is layered, checked in order:
    ///
    /// - OneDrive environment variables (<c>OneDrive</c> / <c>OneDriveCommercial</c>).
    ///
    /// - The Windows Cloud Files API (<c>CfGetSyncRootInfoByPath</c>, available on Windows 10 1709+),
    ///   which detects any provider that registers a sync root (OneDrive, Dropbox, iCloud, Box, ...).
    ///
    /// - Known cloud folder names under the user profile, as a fallback for older systems or
    ///   providers that do not register a sync root.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is inside a cloud-synced folder; false otherwise.</returns>
    public static bool IsPathInCloudSyncFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string fullPath;
        try { fullPath = Path.GetFullPath(path); }
        catch { return false; }

        return IsInOneDrive(fullPath)
            || IsInRegisteredCloudSyncRoot(fullPath)
            || IsInKnownCloudFolder(fullPath);
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

    // --- Tier 3: known cloud folder names under the user profile ---
    private static bool IsInKnownCloudFolder(string fullPath)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(userProfile))
            return false;

        string fullProfile;
        try { fullProfile = Path.GetFullPath(userProfile).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
        catch { return false; }

        foreach (var folder in s_knownCloudFolders)
        {
            try
            {
                if (IsUnder(fullPath, Path.Combine(fullProfile, folder)))
                    return true;
            }
            catch { /* malformed name - skip */ }
        }

        return false;
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

    private static readonly string[] s_knownCloudFolders =
    {
        "Dropbox",
        "Google Drive",
        "GoogleDrive",
        "iCloudDrive",
        "Box Sync",
        "Box",
        "MEGA",
        "pCloud"
    };
}
