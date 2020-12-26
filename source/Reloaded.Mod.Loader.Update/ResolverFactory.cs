using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Resolvers;
using Reloaded.Mod.Loader.Update.Structures;

namespace Reloaded.Mod.Loader.Update
{
    /// <summary>
    /// Factory which returns the first appropriate resolver for a specific mod.
    /// </summary>
    public static class ResolverFactory
    {
        public class Resolvers
        {
            public IModResolver[] All { get; set; }

            public Resolvers(UpdaterData data)
            {
                All = new IModResolver[]
                {
                    // Listed in order of preference.
                    new NugetRepositoryResolver(data),
                    new GitHubLatestUpdateResolver(),
                    new GameBananaUpdateResolver()
                };
            }
        }

        /// <summary>
        /// Returns the first appropriate resolver that can handle updating a mod.
        /// </summary>
        /// <param name="mod">The mod in question.</param>
        /// <param name="data">All data passed to the updater.</param>
        /// <returns>A resolver that can handle the mod, else null.</returns>
        public static IModResolver GetResolver(PathTuple<ModConfig> mod, Structures.UpdaterData data)
        {
            foreach (var resolver in new Resolvers(data).All)
            {
                if (resolver.IsCompatible(mod))
                    return resolver;
            }

            return null;
        }
    }
}
