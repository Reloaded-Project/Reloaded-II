namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Represents all of the data passed to the repository.
/// </summary>
public class UpdaterData
{
    /// <summary>
    /// Contains the repository data.
    /// </summary>
    public List<string> NuGetFeeds { get; set; }

    /// <summary>
    /// Common settings for the package resolvers.
    /// </summary>
    public CommonPackageResolverSettings CommonPackageResolverSettings { get; set; }

    /// <summary/>
    public UpdaterData(List<string> nugetFeeds, CommonPackageResolverSettings commonPackageResolverSettings)
    {
        NuGetFeeds = nugetFeeds;
        CommonPackageResolverSettings = commonPackageResolverSettings;
    }
}