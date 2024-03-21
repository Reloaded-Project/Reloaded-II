namespace Reloaded.Mod.Loader.Update.Index.Structures;

/// <summary>
/// The index file provides a quick lookup for supported sources.
/// </summary>
public class Index
{
    /// <summary>
    /// Map of supported source identifiers.
    /// </summary>
    public Dictionary<string, string> Sources { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Base URL of the site that holds this index.
    /// This is internally created during deserialization.
    /// </summary>
    [JsonIgnore]
    public Uri BaseUrl { get; internal set; } = null!;

    
    /// <summary>
    /// [Slow if over network/internet !!]
    /// Retrieves all the packages from all the sources.
    /// </summary>
    public async Task<PackageList> GetPackagesFromAllSourcesAsync()
    {
        var result = new List<Package>();
        foreach (var source in Sources)
        {
            var fromSource = await Web.DownloadAndDeserialize<PackageList>(new Uri(BaseUrl, source.Value));
            result.AddRange(fromSource.Packages);
        }

        return new PackageList
        {
            Packages = result
        };
    }
    
    /// <summary>
    /// Tries to get the package list for a given NuGet URL.
    /// </summary>
    /// <param name="url">URL to the NuGet repository.</param>
    public async ValueTask<(bool result, PackageList list)> TryGetNuGetPackageList(string url)
    {
        if (!TryGetNuGetSourcePath(url, out var sourcePath))
            return (false, default);

        return (true, await Web.DownloadAndDeserialize<PackageList>(new Uri(BaseUrl, sourcePath)));
    }

    /// <summary>
    /// Tries to get the package list for a given GameBanana URL.
    /// </summary>
    /// <param name="appId">The GameBanana application ID.</param>
    public async ValueTask<(bool result, PackageList list)> TryGetGameBananaPackageList(long appId)
    {
        if (!TryGetGameBananaSourcePath(appId, out var sourcePath))
            return (false, default);

        return (true, await Web.DownloadAndDeserialize<PackageList>(new Uri(BaseUrl, sourcePath)));
    }

    /// <summary>
    /// Tries to get the file path to resolve a NuGet url.
    /// </summary>
    /// <param name="url">URL to the NuGet repository.</param>
    /// <param name="filePath">Relative path to the file containing the GameBanana source.</param>
    /// <returns>The GameBanana source.</returns>
    public bool TryGetNuGetSourcePath(string url, out string? filePath) => Sources.TryGetValue(Routes.Source.GetNuGetIndexKey(url), out filePath);

    /// <summary>
    /// Tries to get the file path to resolve a GameBanana ID.
    /// </summary>
    /// <param name="appId">The GameBanana application ID.</param>
    /// <param name="filePath">Relative path to the file containing the GameBanana source.</param>
    /// <returns>The GameBanana source.</returns>
    public bool TryGetGameBananaSourcePath(long appId, out string? filePath) => Sources.TryGetValue(Routes.Source.GetGameBananaIndex(appId), out filePath);
}