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
        var invalidChars = GetInvalidFileNameChars();
        return new string(fileName.Where(x => !invalidChars.Contains(x)).ToArray());
    }

    // Copied from Windows version, Unix version is a subset of this.
    private static char[] GetInvalidFileNameChars() => new char[]
    {
        '\"', '<', '>', '|', '\0',
        (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
        (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
        (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
        (char)31, ':', '*', '?', '\\', '/'
    };
}