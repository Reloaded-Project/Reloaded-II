using System.ComponentModel;
using System.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Resolvers.GitHub;

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
            LegacyFallbackPattern = githubConfig.AssetFileName
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
        /// [Legacy] Fallback file name pattern if no metadata file is found.
        /// </summary>
        [Category(LegacyCategory)]
        [Description("Pattern for the file name to download if no metadata file is found.\n" +
                     "e.g. *update.zip will look for any file ending with 'update.zip'\n" +
                     "For backwards compatibility only. Do not use with new mods.")]
        public string AssetFileName { get; set; } = "Mod.zip";
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
    }
}