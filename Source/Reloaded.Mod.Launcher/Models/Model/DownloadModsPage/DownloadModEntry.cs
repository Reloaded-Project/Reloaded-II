using NuGet.Versioning;

namespace Reloaded.Mod.Launcher.Models.Model.DownloadModsPage
{
    public class DownloadModEntry
    {
        public string Id            { get; set; }
        public string Description   { get; set; }
        public NuGetVersion Version { get; set; }

        public DownloadModEntry(string id, string description, NuGetVersion version)
        {
            Id = id;
            Description = description;
            Version = version;
        }
    }
}
