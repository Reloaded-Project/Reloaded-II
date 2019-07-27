using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using Onova.Exceptions;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Abstract;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Utilities;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    /// <summary>
    /// Resolver for Github that will download latest release or prerelease.
    /// </summary>
    public class GithubLatestUpdateResolver : IModResolver
    {
        public const int UnregisteredRateLimit = 60;

        // AllowedMods contains the first <UnregisteredRateLimit> mods, ordered by ones checked longest time ago.
        private static HashSet<ModConfig> AllowedMods { get; set; }
        private static GitHubClient GithubClient { get; set; }
        private static int RateLimit => GithubClient.Miscellaneous.GetRateLimits().Result.Resources.Core.Remaining;

        private PathGenericTuple<ModConfig> _modTuple;
        private GithubConfig _githubConfig;
        private GithubUserConfig _githubUserConfig;
        private IReadOnlyList<Release> _releases;
        private ReleaseAsset _targetAsset;

        static GithubLatestUpdateResolver()
        {
            try
            {
                GithubClient    = new GitHubClient(new ProductHeaderValue("Reloaded-II"));

                // Get list of mods allowed to be updated this time around.
                var allMods                 = ModConfig.GetAllMods();
                var allModsWithConfigs      = allMods.Where(x => File.Exists(GithubConfig.GetFilePath(GetModDirectory(x))));

                MakeTimestampsIfNotExist(allModsWithConfigs);

                var orderedModsWithConfigs  = allModsWithConfigs.OrderBy(x => GithubUserConfig.FromPath(GithubUserConfig.GetFilePath(GetModDirectory(x))).LastCheckTimestamp);
                var allowedMods             = orderedModsWithConfigs.Take(UnregisteredRateLimit).Select(x => x.Object);
                AllowedMods                 = new HashSet<ModConfig>(allowedMods);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        /* Interface Implementation */
        public bool IsCompatible(PathGenericTuple<ModConfig> mod)
        {
            try
            {
                if (AssertGithubOK())
                {
                    string path = GithubConfig.GetFilePath(GetModDirectory(mod));

                    if (File.Exists(path) && AllowedMods.Contains(mod.Object))
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public void Construct(PathGenericTuple<ModConfig> mod)
        {
            _modTuple = mod;
            string path = GithubConfig.GetFilePath(GetModDirectory(mod));
            _githubConfig = GithubConfig.FromPath(path);
            _githubUserConfig = GithubUserConfig.FromPath(path);
        }

        public Version GetCurrentVersion()
        {
            return Version.Parse(_modTuple.Object.ModVersion);
        }

        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync()
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
            GithubUserConfig.ToPath(_githubUserConfig, GithubUserConfig.GetFilePath(GetModDirectory(_modTuple)));
        }

        /* Helper classes */
        private static void MakeTimestampsIfNotExist(IEnumerable<PathGenericTuple<ModConfig>> mods)
        {
            foreach (var modWithConfig in mods)
            {
                var configPath = GithubUserConfig.GetFilePath(GetModDirectory(modWithConfig));
                if (!File.Exists(configPath))
                    GithubUserConfig.ToPath(new GithubUserConfig(0), configPath);
            }
        }

        private static string GetModDirectory(PathGenericTuple<ModConfig> mod)
        {
            return Path.GetDirectoryName(mod.Path);
        }

        private bool AssertGithubOK()
        {
            if (GithubClient == null)
                return false;

            if (RateLimit <= 0)
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

        private async Task<IReadOnlyList<Release>> GetReleases(GithubConfig configuration)
        {
            try
            {
                var releases = await GithubClient.Repository.Release.GetAll(configuration.UserName, configuration.RepositoryName);
                if (!_githubUserConfig.EnablePrereleases)
                    releases = releases.Where(x => x.Prerelease == false).ToList();

                return releases;
            }
            catch { return null; }
        }

        /* Github Config. */
        public class GithubUserConfig : JsonSerializable<GithubUserConfig>
        {
            public const string     ConfigFileName = "ReloadedGithubUserConfig.json";
            public static string    GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public long LastCheckTimestamp { get; set; }
            public bool EnablePrereleases  { get; set; } = false;

            public GithubUserConfig() { } // For deserialization
            public GithubUserConfig(long timeStamp)
            {
                LastCheckTimestamp = timeStamp;
            }
        }

        public class GithubConfig : JsonSerializable<GithubConfig>
        {
            public const string     ConfigFileName = "ReloadedGithubUpdater.json";
            public static string    GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public string UserName          { get; set; }
            public string RepositoryName    { get; set; }
            public string AssetFileName     { get; set; } = "Mod.zip";
        }
    }
}
