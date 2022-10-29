using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;
using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Update.Providers.GitHub;

/// <summary>
/// Allows for updating of packages sourced from GitHub releases.
/// </summary>
public class GitHubReleasesUpdateResolverFactory : IUpdateResolverFactory
{
    private static IPackageExtractor _extractor = new SevenZipSharpExtractor();

    /// <inheritdoc />
    public IPackageExtractor Extractor { get; } = _extractor;

    /// <inheritdoc />
    public string ResolverId { get; } = "GitHubRelease";

    /// <inheritdoc />
    public string FriendlyName { get; } = "GitHub Releases";

    /// <inheritdoc/>
    public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig)
    {
        MigrateFromLegacyModConfig(mod);
        MigrateFromLegacyUserConfig(mod, userConfig);
    }

    /// <inheritdoc/>
    public IPackageResolver? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig, UpdaterData data)
    {
        if (!this.TryGetConfiguration<GitHubConfig>(mod, out var githubConfig))
            return null;

        return new GitHubReleaseResolver(new GitHubResolverConfiguration()
        {
            RepositoryName = githubConfig!.RepositoryName,
            UserName = githubConfig.UserName,
            LegacyFallbackPattern = githubConfig.AssetFileName,
            InheritVersionFromTag = githubConfig.UseReleaseTag
        }, data.CommonPackageResolverSettings);
    }
    
    /// <inheritdoc />
    public bool TryGetConfigurationOrDefault(PathTuple<ModConfig> mod, out object configuration)
    {
        bool result = this.TryGetConfiguration<GitHubConfig>(mod, out var config);
        configuration = config ?? new GitHubConfig();
        return result;
    }

    private void MigrateFromLegacyModConfig(PathTuple<ModConfig> mod)
    {
        // Performs migration from legacy separate file config to integrated config.
        var gitHubConfigPath = GitHubConfig.GetFilePath(GetModDirectory(mod));
        if (File.Exists(gitHubConfigPath))
        {
            var githubConfig = IConfig<GitHubConfig>.FromPath(gitHubConfigPath);
            this.SetConfiguration(mod, githubConfig);
            mod.Save();
            IOEx.TryDeleteFile(gitHubConfigPath);
        }
    }

    private void MigrateFromLegacyUserConfig(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig)
    {
        // Performs migration from legacy separate file config to integrated config.
        var gitHubConfigPath = GitHubUserConfig.GetFilePath(GetModDirectory(mod));
        if (File.Exists(gitHubConfigPath) && userConfig != null)
        {
            var githubConfig = IConfig<GitHubUserConfig>.FromPath(gitHubConfigPath);
            userConfig.Config.AllowPrereleases = githubConfig.EnablePrereleases;
            userConfig.Save();
            IOEx.TryDeleteFile(gitHubConfigPath);
        }
    }

    private static string GetModDirectory(PathTuple<ModConfig> mod)
    {
        return Path.GetDirectoryName(mod.Path)!;
    }

    /// <summary>
    /// Stores a configuration describing how to update mod using GitHub.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public class GitHubConfig : IConfig<GitHubConfig>
    {
        private const string DefaultCategory = "GitHub Settings";

        private const string LegacyCategory = "Legacy Settings (Backwards Compatibility)";

        /// <summary/>
        public const string ConfigFileName = "ReloadedGithubUpdater.json";

        /// <summary>
        /// [Legacy] Gets the file path for an existing legacy GitHub config.
        /// </summary>
        public static string GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

        /// <summary>
        /// The user name associated with the repository to fetch files from.
        /// </summary>
        [Category(DefaultCategory)]
        [Description("The user name associated with the repository to fetch files from.\n" +
                     "e.g. TGEnigma for https://github.com/TGEnigma/p4gpc.modloader")]
        public string UserName       { get; set; } = "";

        /// <summary>
        /// The name of the repository to fetch files from.
        /// </summary>
        [Category(DefaultCategory)]
        [Description("The name of the repository to fetch files from.\n" +
                     "e.g. p4gpc.modloader for https://github.com/TGEnigma/p4gpc.modloader")]
        public string RepositoryName { get; set; } = "";

        /// <summary>
        /// Whether to use Release Tag or not.
        /// </summary>
        [Category(DefaultCategory)]
        [Description("Uses the release tag to denote version of the package.\n" +
                     "If false, gets version from Release Metadata file.")]
        public bool UseReleaseTag { get; set; } = true;

        /// <summary>
        /// [Legacy] Fallback file name pattern if no metadata file is found.
        /// </summary>
        [Category(LegacyCategory)]
        [Description("Pattern for the file name to download if no metadata file is found.\n" +
                     "e.g. *update.zip will look for any file ending with 'update.zip'\n" +
                     "For backwards compatibility only. Do not use with new mods.")]
        public string AssetFileName { get; set; } = "Mod.zip";

        // Reflection-free JSON
        /// <inheritdoc />
        public static JsonTypeInfo<GitHubConfig> GetJsonTypeInfo(out bool supportsSerialize)
        {
            supportsSerialize = true;
            return GitHubConfigContext.Default.GitHubConfig;
        }
        
        /// <inheritdoc />
        public JsonTypeInfo<GitHubConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
    }
        
    /// <summary>
    /// Legacy. No longer in use, kept for migration only.
    /// </summary>
    public class GitHubUserConfig : IConfig<GitHubUserConfig>
    {
        /// <summary/>
        public const string ConfigFileName = "ReloadedGithubUserConfig.json";

        /// <summary/>
        public static string GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

        /// <summary>
        /// Overrides the global setting to enable or disable prereleases.
        /// </summary>
        public bool EnablePrereleases { get; set; } = false;

        // Reflection-free JSON
        /// <inheritdoc />
        public static JsonTypeInfo<GitHubUserConfig> GetJsonTypeInfo(out bool supportsSerialize)
        {
            supportsSerialize = true;
            return GitHubUserConfigContext.Default.GitHubUserConfig;
        }
        
        /// <inheritdoc />
        public JsonTypeInfo<GitHubUserConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GitHubReleasesUpdateResolverFactory.GitHubConfig))]
internal partial class GitHubConfigContext : JsonSerializerContext { }


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GitHubReleasesUpdateResolverFactory.GitHubUserConfig))]
internal partial class GitHubUserConfigContext : JsonSerializerContext { }