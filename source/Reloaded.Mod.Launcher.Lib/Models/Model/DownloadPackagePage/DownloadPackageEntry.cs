using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.DownloadPackagePage;

/// <summary>
/// Encapsulates an individual NuGet item to be downloaded.
/// </summary>
public class DownloadPackageEntry
{
    /// <summary>
    /// ID of the mod to be downloaded.
    /// </summary>
    public string Id => Metadata.Identity.Id;

    /// <summary>
    /// Name of the mod to be downloaded.
    /// </summary>
    public string Name => !string.IsNullOrEmpty(Metadata.Title) ? Metadata.Title : Metadata.Identity.Id;
    
    /// <summary>
    /// The mod authors.
    /// </summary>
    public string Authors => Metadata.Authors;

    /// <summary>
    /// Description of the mod.
    /// </summary>
    public string Description => Metadata.Description;
    
    /// <summary>
    /// Version of the mod to download.
    /// </summary>
    public NuGetVersion Version => Metadata.Identity.Version;

    /// <summary>
    /// The NuGet repository where the mod is sourced from.
    /// </summary>
    public INugetRepository Source { get; set; }

    /// <summary>
    /// The complete metadata of the returned package.
    /// </summary>
    public IPackageSearchMetadata Metadata { get; set; }

    /// <summary/>
    public DownloadPackageEntry(IPackageSearchMetadata metadata, INugetRepository source)
    {
        Metadata = metadata;
        Source = source;
    }
}