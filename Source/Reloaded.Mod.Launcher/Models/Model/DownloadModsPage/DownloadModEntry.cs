using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Interfaces;

namespace Reloaded.Mod.Launcher.Models.Model.DownloadModsPage
{
    public class DownloadModEntry
    {
        public string Id            { get; set; }
        public string Authors       { get; set; }
        public string Description   { get; set; }
        public NuGetVersion Version { get; set; }
        public INugetRepository Source { get; set; }

        public DownloadModEntry(string id, string authors, string description, NuGetVersion version, INugetRepository source)
        {
            Id = id;
            Authors = authors;
            Description = description;
            Version = version;
            Source = source;
        }
    }
}
