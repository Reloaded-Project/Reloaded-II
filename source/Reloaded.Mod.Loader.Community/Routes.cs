namespace Reloaded.Mod.Loader.Community;

/// <summary>
/// Contains the list of routes for individual APIs.
/// </summary>
public static class Routes
{
    /// <summary>
    /// File extension.
    /// </summary>
    public const string FileExtension = ".json";

    /// <summary>
    /// File extension (compressed).
    /// </summary>
    public const string CompressionExtension = ".br";

    /// <summary>
    /// Link to the index containing the mappings
    /// </summary>
    public const string Index = $"Index{FileExtension}{CompressionExtension}";

    /// <summary>
    /// Link to the index containing the individual application configs.
    /// </summary>
    public const string Application = "Apps";

    /// <summary>
    /// The application in question.
    /// </summary>
    /// <param name="relativePath">Relative path returned from <see cref="IndexAppEntry"/>.</param>
    public static string GetApplicationPath(string relativePath)
    {
        return $"{Application}/{relativePath}";
    }
}