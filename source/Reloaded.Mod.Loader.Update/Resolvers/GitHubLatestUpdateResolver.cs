﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    /// <summary>
    /// Resolver for GitHub that will download latest release or prerelease.
    /// </summary>
    public class GitHubLatestUpdateResolver : IModResolver
    {
        public const int UnregisteredRateLimit = 60;

        // AllowedMods contains the first <UnregisteredRateLimit> mods, ordered by ones checked longest time ago.
        private static HashSet<ModConfig> AllowedMods { get; set; }
        private static GitHubClient GitHubClient { get; set; }

        private static int          CachedRateLimit { get; set; }
        private static bool         ValidRateLimit  { get; set; } = false;

        private static async Task<int> GetRateLimit()
        {
            if (ValidRateLimit)
                return CachedRateLimit;

            var rateLimits = await GitHubClient.Miscellaneous.GetRateLimits();
            CachedRateLimit = rateLimits.Resources.Core.Remaining;
            ValidRateLimit  = true;

            return CachedRateLimit;
        }

        private PathTuple<ModConfig> _modTuple;
        private GitHubConfig _githubConfig;
        private GitHubUserConfig _githubUserConfig;
        private IReadOnlyList<Release> _releases;
        private ReleaseAsset _targetAsset;

        static GitHubLatestUpdateResolver()
        {
            try
            {
                GitHubClient    = new GitHubClient(new ProductHeaderValue("Reloaded-II"));

                // Get list of mods allowed to be updated this time around.
                var allMods                 = ModConfig.GetAllMods();
                var allModsWithConfigs      = allMods.Where(x => File.Exists(GitHubConfig.GetFilePath(GetModDirectory(x))));

                MakeTimestampsIfNotExist(allModsWithConfigs);

                var orderedModsWithConfigs  = allModsWithConfigs.OrderBy(x => IConfig<GitHubUserConfig>.FromPath(GitHubUserConfig.GetFilePath(GetModDirectory(x))).LastCheckTimestamp);
                var allowedMods             = orderedModsWithConfigs.Take(UnregisteredRateLimit).Select(x => x.Config);
                AllowedMods                 = new HashSet<ModConfig>(allowedMods);
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        /* Interface Implementation */
        public IPackageExtractor Extractor { get; set; } = new ArchiveExtractor();
        public bool IsCompatible(PathTuple<ModConfig> mod)
        {
            try
            {
                if (Task.Run(AssertGitHubOK).Result)
                {
                    string path = GitHubConfig.GetFilePath(GetModDirectory(mod));

                    if (File.Exists(path) && AllowedMods.Contains(mod.Config))
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public void Construct(PathTuple<ModConfig> mod)
        {
            _modTuple = mod;
            string path = GitHubConfig.GetFilePath(GetModDirectory(mod));
            _githubConfig = IConfig<GitHubConfig>.FromPath(path);
            _githubUserConfig = IConfig<GitHubUserConfig>.FromPath(path);
        }

        public Version GetCurrentVersion()
        {
            return Version.Parse(_modTuple.Config.ModVersion);
        }

        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_releases == null)
                _releases = await GetReleases(_githubConfig);

            // Return no mods if rate limit hit.
            if (_releases == null)
                return new List<Version>();

            var versions = new List<Version>();
            foreach (var release in _releases)
            {
                versions.Add(Version.Parse(release.TagName));
            }

            return versions;
        }

        public long GetSize()
        {
            GetTargetAssetIfNull();
            if (_targetAsset != null)
                return _targetAsset.Size;

            return 0;
        }

        public async Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double> progress = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            GetTargetAssetIfNull();
            if (_targetAsset == null)
                throw new Exception("Failed to find asset matching name.");

            var uri     = new Uri(_targetAsset.BrowserDownloadUrl);
            byte[] file = await FileDownloader.DownloadFile(uri, null, DownloadProgressChanged);
            File.WriteAllBytes(destFilePath, file);

            /* Callbacks */
            void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
            {
                progress?.Report((double)args.BytesReceived / args.TotalBytesToReceive);
            }
        }

        public void PostUpdateCallback(bool hasUpdates)
        {
            _githubUserConfig.LastCheckTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            IConfig<GitHubUserConfig>.ToPath(_githubUserConfig, GitHubUserConfig.GetFilePath(GetModDirectory(_modTuple)));
        }

        /* Helper classes */
        private static void MakeTimestampsIfNotExist(IEnumerable<PathTuple<ModConfig>> mods)
        {
            foreach (var modWithConfig in mods)
            {
                var configPath = GitHubUserConfig.GetFilePath(GetModDirectory(modWithConfig));
                if (!File.Exists(configPath))
                    IConfig<GitHubUserConfig>.ToPath(new GitHubUserConfig(0), configPath);
            }
        }

        private static string GetModDirectory(PathTuple<ModConfig> mod)
        {
            return Path.GetDirectoryName(mod.Path);
        }

        private async Task<bool> AssertGitHubOK()
        {
            if (GitHubClient == null)
                return false;

            var rateLimit = await GetRateLimit();
            if (rateLimit <= 0)
                return false;

            return true;
        }

        private void GetTargetAssetIfNull()
        {
            if (_targetAsset == null)
            {
                var release = _releases.First();
                _targetAsset = release.Assets.FirstOrDefault(x => x.Name == _githubConfig.AssetFileName);
            }
        }

        private async Task<IReadOnlyList<Release>> GetReleases(GitHubConfig configuration)
        {
            try
            {
                var releases = await GitHubClient.Repository.Release.GetAll(configuration.UserName, configuration.RepositoryName);
                ValidRateLimit = false;
                if (!_githubUserConfig.EnablePrereleases)
                    releases = releases.Where(x => x.Prerelease == false).ToList();

                return releases;
            }
            catch { return null; }
        }

        /* GitHub Config. */
        public class GitHubUserConfig : IConfig<GitHubUserConfig>
        {
            public const string     ConfigFileName = "ReloadedGithubUserConfig.json";
            public static string    GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public long LastCheckTimestamp { get; set; }
            public bool EnablePrereleases  { get; set; } = false;

            public GitHubUserConfig() { } // For deserialization
            public GitHubUserConfig(long timeStamp)
            {
                LastCheckTimestamp = timeStamp;
            }
        }

        public class GitHubConfig : IConfig<GitHubConfig>
        {
            public const string     ConfigFileName = "ReloadedGithubUpdater.json";
            public static string    GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public string UserName          { get; set; }
            public string RepositoryName    { get; set; }
            public string AssetFileName     { get; set; } = "Mod.zip";
        }
    }
}
