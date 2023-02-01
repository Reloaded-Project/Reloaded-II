using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Aggregate package resolver.
/// </summary>
public class AggregatePackageResolverEx : AggregatePackageResolver
{
    private readonly Dictionary<IPackageResolver, IPackageExtractor> _packageExtractors;

    /// <summary>
    /// Package extractor associated with this resolver.
    /// </summary>
    public ProxyPackageExtractor Extractor { get; private set; } = new(new SevenZipSharpExtractor());

    /// <summary/>
    public AggregatePackageResolverEx(List<IPackageResolver> resolvers, Dictionary<IPackageResolver, IPackageExtractor> packageExtractors) : base(resolvers)
    {
        _packageExtractors = packageExtractors;
        this.OnSuccessfulDownload += SetExtractorOnSuccessfulDownload;
    }

    private void SetExtractorOnSuccessfulDownload(GetResolverResult obj)
    {
        Extractor.Extractor = _packageExtractors.TryGetValue(obj.Resolver, out var extractor) 
            ? extractor 
            : new SevenZipSharpExtractor();
    }
}