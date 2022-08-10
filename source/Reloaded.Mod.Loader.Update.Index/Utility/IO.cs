namespace Reloaded.Mod.Loader.Update.Index.Utility;

/// <summary>
/// Various utility functions for quick I/O operations.
/// </summary>
// ReSharper disable once InconsistentNaming
public class IO
{
    /// <summary>
    /// Sanitizes a file or path name to not contain invalid chars.
    /// </summary>
    /// <returns>Sanitized file name.</returns>
    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(fileName.Where(x => !invalidChars.Contains(x)).ToArray());
    }
}