using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Sewer56.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ResolverModPair
    {
        public AggregatePackageResolver Resolver { get; set; }

        public PathTuple<ModConfig> ModTuple { get; set; }

        public ResolverModPair(AggregatePackageResolver resolver, PathTuple<ModConfig> modTuple)
        {
            Resolver = resolver;
            ModTuple = modTuple;
        }
    }
}
