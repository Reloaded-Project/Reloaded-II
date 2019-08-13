using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    public class NugetRepositoryResolver : IModResolver
    {
        private static NugetHelper _nugetHelper;
        private IPackageSearchMetadata _packageMetadata;
        private ModConfig _modConfig;

        static NugetRepositoryResolver()
        {
            _nugetHelper = Task.Run(() => NugetHelper.FromSourceUrlAsync(SharedConstants.NuGetApiEndpoint)).Result;
        }

        /* Interface Implementation */
        public IPackageExtractor Extractor { get; set; } = new NugetRepositoryExtractor();
        public bool IsCompatible(PathGenericTuple<ModConfig> mod)
        {
            var packages = Task.Run(() => _nugetHelper.GetPackageDetails(mod.Object.ModId, false, true)).Result;
            if (!packages.Any())
                return false;

            _packageMetadata = packages.Last();
            return true;
        }

        public void Construct(PathGenericTuple<ModConfig> mod)
        {
            _modConfig = mod.Object;
        }

        public Version GetCurrentVersion()
        {
            return Version.Parse(_modConfig.ModVersion); ;
        }

        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync()
        {
            try
            {
                return new Version[] { Version.Parse(_packageMetadata.Identity.Version.ToString()) };
            }
            catch (Exception e)
            {
                return new Version[0];
            }
        }

        public long GetSize()
        {
            return -1;
        }

        public async Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var downloadResourceResult = await _nugetHelper.DownloadPackageAsync(_packageMetadata, cancellationToken);
            using (var fileStream = File.OpenWrite(destFilePath))
            {
                downloadResourceResult.PackageStream.CopyTo(fileStream);
            }
        }

        public void PostUpdateCallback(bool hasUpdates) { }
    }
}
