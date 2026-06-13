namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility methods for inspecting file system paths.
/// </summary>
public static class PathUtility
{
    /// <summary>
    /// Checks whether a given path is inside a OneDrive-managed folder.
    /// Uses the <c>OneDrive</c> / <c>OneDriveCommercial</c> environment variables.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is inside a OneDrive root; false otherwise.</returns>
    public static bool IsPathInOneDrive(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        foreach (var envVar in s_oneDriveEnvVars)
        {
            var root = System.Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(root))
                continue;

            try
            {
                var fullRoot = Path.GetFullPath(root)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var fullPath = Path.GetFullPath(path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch { /* malformed path - skip */ }
        }

        return false;
    }

    private static readonly string[] s_oneDriveEnvVars = { "OneDrive", "OneDriveCommercial" };
}
