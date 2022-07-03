namespace Reloaded.Mod.Loader.Community.Utility;

/// <summary>
/// Utility methods related to hashing files.
/// </summary>
public static class Hashing
{
    /// <summary>
    /// Converts the specified hash into a string.
    /// </summary>
    /// <param name="hash">The hash.</param>
    public static string ToString(ulong hash) => hash.ToString("X");
}