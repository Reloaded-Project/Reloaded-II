using System.IO;
using System.Linq;

public static class PathSanitizer
{
    private static readonly char[] Invalid = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

    /// <summary>
    /// Removes invalid characters from a specified file path.
    /// </summary>
    public static string ForceValidFilePath(string text)
    {
        foreach (char c in Invalid)
        {
            if (c != '\\' || c != '/')
                text = text.Replace(c.ToString(), "");
        }

        return text;
    }
}