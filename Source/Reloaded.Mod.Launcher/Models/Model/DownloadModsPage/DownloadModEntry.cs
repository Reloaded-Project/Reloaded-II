using System;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;

namespace Reloaded.Mod.Launcher.Models.Model.DownloadModsPage
{
    public class DownloadModEntry
    {
        public string Id => Metadata.Identity.Id;
        public string Name => !string.IsNullOrEmpty(Metadata.Title) ? Metadata.Title : Metadata.Identity.Id;
        public string Authors => Metadata.Authors;
        public string Description => Metadata.Description;
        public NuGetVersion Version => Metadata.Identity.Version;
        public INugetRepository Source { get; set; }
        public IPackageSearchMetadata Metadata { get; set; }

        public DownloadModEntry(IPackageSearchMetadata metadata, INugetRepository source)
        {
            Metadata = metadata;
            Source = source;
        }
    }
}
