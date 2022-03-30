using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Interfaces.Extensions;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update.Providers;

/// <summary>
/// Aggregate package resolver.
/// </summary>
public class AggregatePackageResolverEx : AggregatePackageResolver, IPackageResolver
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