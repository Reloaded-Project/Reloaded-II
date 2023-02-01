#pragma warning disable CS1591

namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces;

public class NetCoreDependency : IDependency
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool Available => Result.Available;

    /// <inheritdoc />
    public Architecture Architecture { get; }

    public DependencySearchResult<FrameworkOptionsTuple, Framework> Result { get; }

    public NetCoreDependency(string name, DependencySearchResult<FrameworkOptionsTuple, Framework> result, Architecture architecture)
    {
        Name = name;
        Result = result;
        Architecture = architecture;
    }

    /// <inheritdoc />
    public async Task<string[]> GetUrlsAsync()
    {
        if (Result.Available) 
            return new[] {""};
            
        var urls = new List<string>();
        foreach (var dependency in Result.MissingDependencies)
        {
            string url;
            try
            {
                var downloader = new FrameworkDownloader(dependency.NuGetVersion, dependency.FrameworkName);
                url = await downloader.GetDownloadUrlAsync(Architecture, Platform.Windows, Format.Executable, true);
            }
            catch (Exception) { url = ""; }

            urls.Add(url);
        }

        return urls.ToArray();

    }
}