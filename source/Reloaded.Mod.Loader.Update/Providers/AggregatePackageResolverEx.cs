using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Aggregate package resolver.
/// </summary>
public class AggregatePackageResolverEx : AggregatePackageResolver
{
    private readonly Dictionary<IPackageResolver, IPackageExtractor> _packageExtractors;

    /// <summary/>
    public AggregatePackageResolverEx(List<IPackageResolver> resolvers, Dictionary<IPackageResolver, IPackageExtractor> packageExtractors) : base(resolvers)
    {
        _packageExtractors = packageExtractors;
    }

    /// <summary>
    /// Retrieves the package extractor to be used for removing packages.
    /// </summary>
    public async Task<IPackageExtractor> GetExtractorAsync(NuGetVersion version)
    {
        var resolver = await GetResolverForVersionAsync(version, CancellationToken.None);
        if (_packageExtractors.TryGetValue(resolver.Resolver, out var extractor))
            return extractor;

        // Fallback
        return new SevenZipSharpExtractor();
    }
}