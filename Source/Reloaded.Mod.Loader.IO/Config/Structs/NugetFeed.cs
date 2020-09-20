using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Loader.IO.Config.Structs
{
    public class NugetFeed
    {
        /// <summary>
        /// Name for the NuGet Feed.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the NuGet API endpoint e.g. <see cref="SharedConstants.NuGetApiEndpoint"/>
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// [Optional] Description of the feed.
        /// </summary>
        public string Description { get; set; }

        public NugetFeed() { }

        public NugetFeed(string name, string url)
        {
            Name = name;
            URL = url;
        }

        public NugetFeed(string name, string url, string description)
        {
            Name = name;
            URL = url;
            Description = description;
        }
    }
}
