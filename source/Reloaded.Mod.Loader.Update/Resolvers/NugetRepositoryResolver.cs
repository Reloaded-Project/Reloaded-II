using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    public class NugetRepositoryResolver : IModResolver
    {
        private static AggregateNugetRepository _nugetRepository;
        private NugetTuple<IPackageSearchMetadata> _metadata;
        private ModConfig _modConfig;

        public NugetRepositoryResolver(UpdaterData data)
        {
            _nugetRepository = data.AggregateNugetRepository;
        }

        /* Interface Implementation */
        public IPackageExtractor Extractor { get; set; } = new NugetRepositoryExtractor();
        public bool IsCompatible(PathTuple<ModConfig> mod)
        {
            var package = Task.Run(() => _nugetRepository.GetLatestPackageDetails(mod.Config.ModId, false, true)).Result;
            if (package == null)
                return false;

            _metadata = package;
            return true;
        }

        public void Construct(PathTuple<ModConfig> mod)
        {
            _modConfig = mod.Config;
        }

        public Version GetCurrentVersion()
        {
            return Version.Parse(_modConfig.ModVersion); ;
        }

        #pragma warning disable 1998
        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default)
        #pragma warning restore 1998
        {
            try
            {
                return new Version[] { Version.Parse(_metadata.Generic.Identity.Version.ToString()) };
            }
            catch (Exception)
            {
                return new Version[0];
            }
        }

        public long GetSize()
        {
            return -1;
        }

        public async Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            var downloadResourceResult = await _metadata.Repository.DownloadPackageAsync(_metadata.Generic, cancellationToken);
            using (var fileStream = File.OpenWrite(destFilePath))
            {
                downloadResourceResult.PackageStream.CopyTo(fileStream);
            }
        }

        public void PostUpdateCallback(bool hasUpdates) { }
    }
}
