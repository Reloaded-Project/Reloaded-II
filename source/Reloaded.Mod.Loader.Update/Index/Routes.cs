namespace Reloaded.Mod.Loader.Update.Index;

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
    /// Link to the index containing all packages
    /// </summary>
    public const string AllPackages = $"AllPackages{FileExtension}{CompressionExtension}";

    /// <summary>
    /// Link to the index containing all dependencies
    /// </summary>
    /// <remarks>
    ///     This is same as <see cref="AllPackages"/> but with information omitted to reduce bandwidth.
    /// </remarks>
    public const string AllDependencies = $"AllDependencies{FileExtension}{CompressionExtension}";

    /// <summary>
    /// Contains the code to determine locations of output files
    /// </summary>
    public static class Build
    {
        /// <summary>
        /// Link to expected path for a GameBanana app.
        /// </summary>
        public static string GetGameBananaPackageListPath(long appId) => $"Search/GameBanana/{appId}/Index{FileExtension}{CompressionExtension}";

        /// <summary>
        /// Link to expected path for a NuGet repository.
        /// </summary>
        public static string GetNuGetPackageListPath(string url) => $"Search/NuGet/{Utility.IO.SanitizeFileName(url)}/Index{FileExtension}{CompressionExtension}";
    }

    /// <summary>
    /// Contains the code to determine index keys for given sources.
    /// </summary>
    public static class Source
    {
        /// <summary>
        /// Prefix used for storing GameBanana packages.
        /// </summary>
        public const string IdentifierGameBanana = "GB/";

        /// <summary>
        /// Prefix used for storing NuGet packages.
        /// </summary>
        public const string IdentifierNuGet = "NuGet/";

        /// <summary>
        /// Gets the ID that would be used for representing a GameBanana entry.
        /// </summary>
        /// <param name="appId">Application id from GameBanana.</param>
        public static string GetGameBananaIndex(long appId) => $"{IdentifierGameBanana}{appId}";

        /// <summary>
        /// Gets the ID that would be used for representing a NuGet entry.
        /// </summary>
        /// <param name="url">URL to the NuGet V3 index.</param>
        public static string GetNuGetIndexKey(string url) => $"{IdentifierNuGet}{url}";
    }
}