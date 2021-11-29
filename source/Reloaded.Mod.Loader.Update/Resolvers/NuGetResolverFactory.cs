using System.Collections.Generic;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Resolvers;
using Sewer56.Update.Resolvers.NuGet;
using Sewer56.Update.Resolvers.NuGet.Utilities;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    public class NuGetResolverFactory : IResolverFactory
    {
        public string ResolverId { get; } = "NuGet";

        /// <inheritdoc/>
        public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig) { }

        /// <inheritdoc/>
        public IPackageResolver GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig, UpdaterData data)
        {
            var resolvers = new List<IPackageResolver>();

            foreach (var source in data.NuGetFeeds)
            {
                resolvers.Add(new NuGetUpdateResolver(
                    new NuGetUpdateResolverSettings()
                    {
                        AllowUnlisted = false,
                        NugetRepository = new NugetRepository(source),
                        PackageId = mod.Config.ModId
                    },
                    new CommonPackageResolverSettings()
                    {
                        AllowPrereleases = data.CommonPackageResolverSettings.AllowPrereleases,
                        MetadataFileName = data.CommonPackageResolverSettings.MetadataFileName
                    }
                ));
            }

            if (resolvers.Count > 0)
                return new AggregatePackageResolver(resolvers);
            
            return null;
        }
    }
}
