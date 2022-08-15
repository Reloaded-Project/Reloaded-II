namespace Reloaded.Mod.Loader.Community.Utility;

/// <summary>
/// Utility methods related to hashing files.
/// </summary>
public static class Hashing
{
    /// <summary>
    /// Converts a stream from hash.
    /// </summary>
    /// <param name="stream">Stream to compute a hash from.</param>
    public static async ValueTask<ulong> FromStreamAsync(Stream stream) => await xxHash64.ComputeHashAsync(stream);

    /// <summary>
    /// Converts the specified hash into a string.
    /// </summary>
    /// <param name="hash">The hash.</param>
    public static string ToString(ulong hash) => hash.ToString("X");
}