namespace Reloaded.Mod.Loader.Community.Utility;

/// <summary>
/// Various utility functions for quick I/O operations.
/// </summary>
// ReSharper disable once InconsistentNaming
public class IO
{
    /// <summary>
    /// Retrieves a relative path for a file given a folder name.
    /// </summary>
    /// <param name="fullPath">Full path for the file.</param>
    /// <param name="folderPath">The folder to get the path relative to.</param>
    public static string GetRelativePath(string fullPath, string folderPath) => fullPath.Substring(folderPath.Length + 1);
}