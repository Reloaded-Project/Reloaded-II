namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Represents a package that can be downloaded.
/// </summary>
public interface IDownloadablePackage : INotifyPropertyChanged
{
    /// <summary>
    /// Name of the mod to be downloaded.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Source of the package.
    /// Which provider the package comes from.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Id of the mod to be downloaded.
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// The mod authors. As displayed in launcher.
    /// Used for credits.
    /// </summary>
    public string? Authors { get; }

    /// <summary>
    /// Contains information about the person who uploaded this item.
    /// </summary>
    public Submitter? Submitter { get; }

    /// <summary>
    /// Short description of the mod, as seen in mod config menu.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Version of the mod to download.
    /// </summary>
    public NuGetVersion? Version { get; }

    /// <summary>
    /// File size in bytes of the item to be downloaded.
    /// </summary>
    public long? FileSize { get; }

    /// <summary>
    /// Provides a human readable readme file, in markdown format.
    /// </summary>
    public string? MarkdownReadme { get; }

    /// <summary>
    /// Long description of the mod.
    /// </summary>
    public string? LongDescription  => !string.IsNullOrEmpty(MarkdownReadme) ? MarkdownReadme : Description;

    /// <summary>
    /// Provides a list of images for this package.
    /// </summary>
    public DownloadableImage[]? Images { get; }

    /// <summary>
    /// The URL of the web page of the project.
    /// </summary>
    public Uri? ProjectUri { get; }

    /// <summary>
    /// Number of likes/upvotes for this package.
    /// </summary>
    public long? LikeCount { get; }

    /// <summary>
    /// Number of views for this package.
    /// </summary>
    public long? ViewCount { get; }

    /// <summary>
    /// Number of downloads for this package.
    /// </summary>
    public long? DownloadCount { get; }

    /// <summary>
    /// Time when this package was published.
    /// In UTC.
    /// </summary>
    public DateTime? Published { get; }

    /// <summary>
    /// Changelog for this package.
    /// </summary>
    public string? Changelog { get; }

    /// <summary>
    /// Collection of tags related to this package.
    /// Tags are optional and their usage depends on implementation.
    ///
    /// For example NuGet uses tags to filter games.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Downloads the package in question asynchronously.
    /// </summary>
    /// <param name="packageFolder">The folder containing all the packages.</param>
    /// <param name="progress">Provides progress reporting for the download operation.</param>
    /// <param name="token">Allows you to cancel the operation.</param>
    /// <returns>Folder where the package was downloaded.</returns>
    public Task<string> DownloadAsync(string packageFolder, IProgress<double>? progress, CancellationToken token = default);
}

/// <summary>
/// Represents an image that can be downloaded from the web for this package.
/// </summary>
public struct DownloadableImage : INotifyPropertyChanged
{
    /// <summary>
    /// Provides an URL to the image.
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// Caption to display under the image.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Provides additional thumbnails for this image.
    /// </summary>
    public DownloadableImageThumbnail[]? Thumbnails { get; set; }

    /// <summary>
    /// Selects an image to display based on a width hint.
    /// </summary>
    /// <param name="width">Default value of max will pick the highest quality.</param>
    public Uri SelectBasedOnWidth(int width = int.MaxValue)
    {
        if (width == int.MaxValue || Thumbnails == null || Thumbnails.Length == 0)
            return Uri;

        foreach (var thumbnail in Thumbnails)
        {
            if (thumbnail.WidthHint == null)
                continue;

            if (thumbnail.WidthHint > width)
                return thumbnail.Uri;
        }

        return Uri;
    }

    // Binding
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Represents a thumbnail for downloadable image.
/// </summary>
public struct DownloadableImageThumbnail : INotifyPropertyChanged
{
    /// <summary>
    /// Represents a thumbnail for a downloadable image.
    /// </summary>
    /// <param name="uri">Full URI to the image.</param>
    /// <param name="widthHint">Hint about the width of the image.</param>
    public DownloadableImageThumbnail(Uri uri, short? widthHint = null)
    {
        Uri = uri;
        WidthHint = widthHint;
    }

    /// <summary>
    /// Provides an URL to the image.
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// Provides a hint regarding the width of an image.
    /// Used for picking images without downloading them.
    /// </summary>
    public short? WidthHint { get; set; }

    // Binding
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Represents the submitter of the downloadable item, i.e. the person that uploaded it.
/// </summary>
public struct Submitter : INotifyPropertyChanged
{
    // TODO: [NET7] Restore constructor with guarantee of non-null username. Right now we can't because it breaks built-in System.Text.Json. This is fixed in newer versions.

    /// <summary>
    /// Name of the submitter.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Provides an URL to the user's avatar.
    /// </summary>
    public Uri? AvatarUrl { get; set; }

    /// <summary>
    /// Date of when the user has joined.
    /// In UTC.
    /// </summary>
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// URL of the user created profile.
    /// </summary>
    public Uri? ProfileUrl { get; set; }

    // For binding
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Extensions for downloadable packages.
/// </summary>
public static class IDownloadablePackageExtensions 
{
    /// <summary>
    /// Applies sorting filters to output packages.
    /// </summary>
    public static IEnumerable<IDownloadablePackage> ApplyFilters(this IEnumerable<IDownloadablePackage> items, SearchSorting sort, bool isDescending)
    {
        if (sort == SearchSorting.None)
            return items;

        switch (sort)
        {
            case SearchSorting.LastModified:
                return isDescending
                    ? items.OrderByDescending(x => x.Published)
                    : items.OrderBy(x => x.Published);

            case SearchSorting.Downloads:
                return isDescending
                    ? items.OrderByDescending(x => x.DownloadCount)
                    : items.OrderBy(x => x.DownloadCount);

            case SearchSorting.Likes:
                return isDescending
                    ? items.OrderByDescending(x => x.LikeCount)
                    : items.OrderBy(x => x.LikeCount);

            case SearchSorting.Views:
                return isDescending
                    ? items.OrderByDescending(x => x.ViewCount)
                    : items.OrderBy(x => x.ViewCount);

            case SearchSorting.None:
                return items;
        }

        return items;
    }
}