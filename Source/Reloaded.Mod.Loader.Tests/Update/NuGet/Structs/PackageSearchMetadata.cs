using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet.Structs
{
    public class CustomPackageSearchMetadata : IPackageSearchMetadata
    {
        public CustomPackageSearchMetadata(PackageIdentity identity)
        {
            Identity = identity;
        }

        /// <inheritdoc />
        public Task<PackageDeprecationMetadata> GetDeprecationMetadataAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public Task<IEnumerable<VersionInfo>> GetVersionsAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public string Authors { get; set; }

        /// <inheritdoc />
        public IEnumerable<PackageDependencyGroup> DependencySets { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public long? DownloadCount { get; set; }

        /// <inheritdoc />
        public Uri IconUrl { get; set; }

        /// <inheritdoc />
        public PackageIdentity Identity { get; set; }

        /// <inheritdoc />
        public Uri LicenseUrl { get; set; }

        /// <inheritdoc />
        public Uri ProjectUrl { get; set; }

        /// <inheritdoc />
        public Uri ReportAbuseUrl { get; set; }

        /// <inheritdoc />
        public Uri PackageDetailsUrl { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? Published { get; set; }

        /// <inheritdoc />
        public string Owners { get; set; }

        /// <inheritdoc />
        public bool RequireLicenseAcceptance { get; set; }

        /// <inheritdoc />
        public string Summary { get; set; }

        /// <inheritdoc />
        public string Tags { get; set; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public bool IsListed { get; set; }

        /// <inheritdoc />
        public bool PrefixReserved { get; set; }

        /// <inheritdoc />
        public LicenseMetadata LicenseMetadata { get; set; }
    }
}
