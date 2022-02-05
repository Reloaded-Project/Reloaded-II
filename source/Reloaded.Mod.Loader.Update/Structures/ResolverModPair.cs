using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Providers;
using Sewer56.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Tuple class that binds together a package resolver and a mod.
/// </summary>
public class ResolverModPair
{
    /// <summary>
    /// The package resolver.
    /// </summary>
    public AggregatePackageResolverEx Resolver { get; set; }

    /// <summary>
    /// The individual mod bound to the resolver.
    /// </summary>
    public PathTuple<ModConfig> ModTuple { get; set; }

    /// <summary/>
    public ResolverModPair(AggregatePackageResolverEx resolver, PathTuple<ModConfig> modTuple)
    {
        Resolver = resolver;
        ModTuple = modTuple;
    }
}