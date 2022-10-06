using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Service that provides caching support for fetching images via URL.
/// Used as a singleton.
/// </summary>
public class ImageCacheService : AkavacheCache
{
    /// <summary>
    /// Gets the time a mod preview should expire from current time.
    /// </summary>
    public TimeSpan ModPreviewExpiration => TimeSpan.FromDays(14);

    /// <summary>
    /// Gets the time a an image should expire by default.
    /// </summary>
    public TimeSpan DefaultExpiration => TimeSpan.FromDays(14);

    /// <summary/>
    public ImageCacheService() : base(Path.Combine(Paths.ImageCachePath)) { }
}