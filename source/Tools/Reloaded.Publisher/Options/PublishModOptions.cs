namespace Reloaded.Publisher.Options
{
    [Verb("Publish", HelpText = "Creates a new release or updates an existing release.")]
    internal class PublishModOptions
    {
        [Option(Required = true, HelpText = "Folder where to output the created packages. Specify a folder containing an existing release will update it.")]
        public string OutputFolder { get; set; } = null!;

        [Option(Required = true, HelpText = "Path of the folder containing a ModConfig.json")]
        public string ModFolder { get; set; } = null!;

        [Option(Required = true, HelpText = "Allows you to set a custom name for the resulting 7z files.")]
        public string? PackageName { get; set; } = null;

        [Option(Required = false, HelpText = "Paths to older versions of the mod. Used for creating delta packages.")]
        public IEnumerable<string> OlderVersionFolders { get; set; } = Array.Empty<string>();

        [Option(Required = false, HelpText = "Automatically creates a delta package when building updating an existing release.", Default = false)]
        public bool AutomaticDelta { get; set; }

        [Option(Required = false, HelpText = "Regexes of files to make sure are ignored.", Default = new []
        {
            @".*\.json", // Config files
            @".*\.nuspec"
        })]
        public IEnumerable<string> IgnoreRegexes { get; set; } = Array.Empty<string>();
        
        [Option(Required = false, HelpText = "Regexes of files to make sure are included.", Default = new[]
        {
            @"ModConfig\.json", // Mod config file.
            @"\.deps\.json",
            @"\.runtimeconfig\.json"
        })]
        public IEnumerable<string> IncludeRegexes { get; set; } = Array.Empty<string>();

        [Option(Required = false, HelpText = "Target for Publishing. Do not change unless instructed.", Default = Mod.Loader.Update.Packaging.Publisher.PublishTarget.Default)]
        public Mod.Loader.Update.Packaging.Publisher.PublishTarget PublishTarget { get; set; } = Mod.Loader.Update.Packaging.Publisher.PublishTarget.Default;

        [Option(Required = false, HelpText = "The amount by which the mod should be compressed.", Default = CompressionLevel.Ultra)]
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Ultra;

        [Option(Required = false, HelpText = "The method with which the archive should be compressed by.", Default = CompressionMethod.Lzma2)]
        public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.Lzma2;

        [Option(Required = false, HelpText = "File path to the file containing the changelog for the package, in Markdown format.")]
        public string? ChangelogPath { get; set; } = null;

        [Option(Required = false, HelpText = "File path to the file containing the readme for the package, in Markdown format. The readme is displayed in the download mods page.")]
        public string? ReadmePath { get; set; } = null;
    }
}