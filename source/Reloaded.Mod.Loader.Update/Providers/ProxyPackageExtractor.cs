namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Package extractor that wraps a mutable inner extractor.
/// </summary>
public class ProxyPackageExtractor : IPackageExtractor
{
    /// <summary>
    /// The extractor wrapped by this proxy.
    /// </summary>
    public IPackageExtractor Extractor { get; set; }

    /// <summary>
    /// A proxy package extractor what wraps an external extractor.
    /// </summary>
    public ProxyPackageExtractor(IPackageExtractor extractor) => Extractor = extractor;

    /// <inheritdoc />
    public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath, IProgress<double>? progress = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await Extractor.ExtractPackageAsync(sourceFilePath, destDirPath, progress, cancellationToken);
    }
}