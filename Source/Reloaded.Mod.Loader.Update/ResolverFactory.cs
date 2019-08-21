using System.Collections.Generic;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Resolvers;

namespace Reloaded.Mod.Loader.Update
{
    /// <summary>
    /// Factory which returns the first appropriate resolver for a specific mod.
    /// </summary>
    public static class ResolverFactory
    {
        /// <summary>
        /// List of all resolvers available in this package.
        /// </summary>
        private class ResolverCollection
        {
            public IModResolver[] Resolvers { get; set; } = { new NugetRepositoryResolver(), new GithubLatestUpdateResolver(), new GameBananaUpdateResolver() };
        }

        /// <summary>
        /// Returns the first appropriate resolver that can handle updating a mod.
        /// </summary>
        /// <param name="mod">The mod in question.</param>
        /// <returns>A resolver that can handle the mod, else null.</returns>
        public static IModResolver GetResolver(PathGenericTuple<ModConfig> mod)
        {
            foreach (var resolver in new ResolverCollection().Resolvers)
            {
                if (resolver.IsCompatible(mod))
                    return resolver;
            }

            return null;
        }
    }
}
