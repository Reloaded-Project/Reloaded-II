using PackageType = Sewer56.Update.Packaging.Enums.PackageType;

namespace Reloaded.Mod.Loader.Update.Packaging;

/// <summary>
/// Class used for creating library compatible updates for Reloaded packages.
/// </summary>
public static class Publisher
{
    private const string GamebananaDeltaIdentifier = ".disable_gb1click";

    /// <summary>
    /// Asynchronously publishes a package.
    /// </summary>
    /// <returns>Task which completes alongside the creation of the package.</returns>
    public static async Task<ReleaseMetadata> PublishAsync(PublishArgs args)
    {
        // Validation
        var ignoreRegexesList  = args.IgnoreRegexes;
        var includeRegexesList = args.IncludeRegexes;
        ignoreRegexesList.Add(Regex.Escape(args.MetadataFileName));

        // Arrange
        var builder = new ReleaseBuilder<Empty>();
        builder.AddCopyPackage(new CopyBuilderItem<Empty>()
        {
            FolderPath = Path.GetDirectoryName(args.ModTuple.Path)!,
            Version = args.ModTuple.Config.ModVersion,
            IgnoreRegexes = ignoreRegexesList,
            IncludeRegexes = includeRegexesList
        });

        foreach (var olderFolder in args.OlderVersionFolders)
        {
            var olderPath = Path.Combine(olderFolder, ModConfig.ConfigFileName);
            var config = await IConfig<ModConfig>.FromPathAsync(olderPath);

            builder.AddDeltaPackage(new DeltaBuilderItem<Empty>()
            {
                Version = args.ModTuple.Config.ModVersion,
                PreviousVersion = config.ModVersion,
                FolderPath = Path.GetDirectoryName(args.ModTuple.Path)!,
                PreviousVersionFolder = olderFolder,
                IgnoreRegexes = ignoreRegexesList,
                IncludeRegexes = includeRegexesList
            });
        }

        // Act
        IPackageArchiver archiver = new SevenZipSharpArchiver(new SevenZipSharpArchiverSettings()
        {
            ArchiveFormat = OutArchiveFormat.SevenZip,
            CompressionLevel = args.CompressionLevel,
            CompressionMethod = args.CompressionMethod,
            UseFastCompression = false
        });

        var compressionScheme = args.PublishTarget == PublishTarget.GameBanana ? JsonCompression.None : JsonCompression.Brotli;
        if (args.PublishTarget == PublishTarget.NuGet)
        {
            archiver = new NuGetPackageArchiver(new NuGetPackageArchiverSettings()
            {
                Id = args.ModTuple.Config.ModId,
                Description = args.ModTuple.Config.ModDescription,
                Authors = new List<string>() { args.ModTuple.Config.ModAuthor },
                OnPreBuild = packageBuilder =>
                {
                    packageBuilder.Title = args.ModTuple.Config.ModName;

                    // Add readme
                    if (!string.IsNullOrEmpty(args.ReadmePath))
                    {
                        const string readmeFilePath = "README.md";
                        packageBuilder.Readme = readmeFilePath;
                        packageBuilder.Files.Add(new PhysicalPackageFile()
                        {
                            SourcePath = args.ReadmePath,
                            TargetPath = readmeFilePath
                        });
                    }

                    // Add icon
                    if (args.ModTuple.Config.TryGetIconPath(args.ModTuple.Path, out var iconToPack))
                    {
                        const string iconFilePath = "icon.png";
                        packageBuilder.Icon = iconFilePath;
                        packageBuilder.Files.Add(new PhysicalPackageFile()
                        {
                            SourcePath = iconToPack,
                            TargetPath = iconFilePath
                        });
                    }

                    // Add changelog
                    if (!string.IsNullOrEmpty(args.ChangelogPath))
                        packageBuilder.ReleaseNotes = File.ReadAllText(args.ChangelogPath);

                    // Add supported games to packages.
                    foreach (var supportedGame in args.ModTuple.Config.SupportedAppId)
                        packageBuilder.Tags.Add(supportedGame);

                    var mods = args.ModTuple.Config.ModDependencies.Select(x => new PackageDependency(x, VersionRange.All));
                    packageBuilder.DependencyGroups.Add(new PackageDependencyGroup(NuGetFramework.AnyFramework, mods));
                } 
            });
        }

        var fileName  = string.IsNullOrEmpty(args.PackageName) ? args.ModTuple.Config.ModName.Replace(' ', '_') : args.PackageName;
        var extraData = new ReleaseMetadataExtraData()
        {
            ModId   = args.ModTuple.Config.ModId,
            ModName = args.ModTuple.Config.ModName,
            ModDescription = args.ModTuple.Config.ModDescription,
        };

        if (!string.IsNullOrEmpty(args.ChangelogPath))
            extraData.Changelog = await File.ReadAllTextAsync(args.ChangelogPath);

        if (!string.IsNullOrEmpty(args.ReadmePath))
            extraData.Readme = await File.ReadAllTextAsync(args.ReadmePath);

        var buildArgs = new BuildArgs()
        {
            FileName = fileName,
            OutputFolder = args.OutputFolder,
            PackageArchiver = archiver,
            PackageExtractor = new SevenZipSharpExtractor(),
            AutoGenerateDelta = args.AutomaticDelta,
            ReleaseExtraData = extraData,
            JsonCompressionMode = compressionScheme,
            MetadataFileName = args.MetadataFileName
        };

        if (args.PublishTarget == PublishTarget.GameBanana)
            buildArgs.FileNameFilter = GameBananaUtilities.SanitizeFileName;

        var metadata = await builder.BuildAsync(buildArgs, args.Progress);

        // Add GameBanana integration marker on deltas.
        if (args.PublishTarget == PublishTarget.GameBanana)
        {
            using var temporaryDirectory = new TemporaryFolderAllocation();
            var bananaIdentifierPath = Path.Combine(temporaryDirectory.FolderPath, GamebananaDeltaIdentifier);
            await File.Create(bananaIdentifierPath).DisposeAsync();

            foreach (var release in metadata.Releases)
            {
                if (release.ReleaseType != PackageType.Delta)
                    continue;

                var releaseFilePath = Path.Combine(args.OutputFolder, release.FileName);
                AddNoBananaAsync(releaseFilePath, bananaIdentifierPath);
            }
        }

        return metadata;
    }

    private static void AddNoBananaAsync(string archiveFilePath, string identifierFilePath)
    {
        using var extractor = new SevenZipExtractor(archiveFilePath);
        bool containsBanana = extractor.ArchiveFileNames.Contains(GamebananaDeltaIdentifier);
        if (containsBanana)
            return;

        // Add the no banana indicator.
        var compressor = new SevenZipCompressor()
        {
            CompressionMode = CompressionMode.Append,
            IncludeEmptyDirectories = false,
            DirectoryStructure = true
        };

        var dictionary = new Dictionary<string, string>() { { GamebananaDeltaIdentifier, identifierFilePath } };
        compressor.CompressFileDictionary(dictionary, archiveFilePath);
    }

    /// <summary>
    /// Arguments used for the publish command.
    /// </summary>
    public class PublishArgs
    {
        /// <summary>
        /// Target for publishing the package to.
        /// </summary>
        public PublishTarget PublishTarget { get; set; }

        /// <summary>
        /// Folder where to output the created packages.
        /// </summary>
        public string OutputFolder { get; set; } = null!;

        /// <summary>
        /// Sets the file name for the release metadata file.
        /// </summary>
        public string MetadataFileName { get; set; } = ReleaseMetadata.DefaultFileName;

        /// <summary>
        /// The mod configuration for which to generate the config for.
        /// </summary>
        public PathTuple<ModConfig> ModTuple { get; set; } = null!;

        /// <summary>
        /// Allows you to set a custom name for the resulting 7z files.
        /// </summary>
        public string? PackageName { get; set; }

        /// <summary>
        /// Can be used to optionally report progress.
        /// </summary>
        public IProgress<double>? Progress { get; set; }

        /// <summary>
        /// Paths to older versions of the same mods.
        /// </summary>
        public List<string> OlderVersionFolders { get; set; } = new();

        /// <summary>
        /// Regexes of files to make sure are ignored.
        /// </summary>
        public List<string> IgnoreRegexes { get; set; } = new();

        /// <summary>
        /// Regexes of files to make sure are not ignored.
        /// </summary>
        public List<string> IncludeRegexes { get; set; } = new();

        /// <summary>
        /// Automatically creates a delta package when building updating an existing release.
        /// </summary>
        public bool AutomaticDelta { get; set; }

        /// <summary>
        /// The amount by which the mod should be compressed.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Ultra;

        /// <summary>
        /// The method with which the archive should be compressed by.
        /// </summary>
        public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.Lzma2;

        /// <summary>
        /// Path to the changelog to use for this release.
        /// </summary>
        public string? ChangelogPath { get; set; } = null;

        /// <summary>
        /// Path to the readme to use for this release.
        /// </summary>
        public string? ReadmePath { get; set; } = null;
    }

    /// <summary>
    /// Target to publish the package.
    /// </summary>
    public enum PublishTarget
    {
        /// <summary>
        /// Upload to any non-NuGet site.
        /// </summary>
        Default,

        /// <summary>
        /// Upload package to NuGet.
        /// </summary>
        NuGet,

        /// <summary>
        /// Upload package to GameBanana.
        /// </summary>
        GameBanana,
    }
}