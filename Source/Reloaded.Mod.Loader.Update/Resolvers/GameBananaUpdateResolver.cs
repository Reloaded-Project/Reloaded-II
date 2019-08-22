using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Abstract;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Resolvers.GameBanana;

namespace Reloaded.Mod.Loader.Update.Resolvers
{
    public class GameBananaUpdateResolver : IModResolver
    {
        public PathGenericTuple<ModConfig> Mod  { get; set; }
        public string ConfigPath                { get; set; }
        public GameBananaConfig Config          { get; set; }
        public GameBananaItem Item              { get; set; }
        public GameBananaItemFile ItemFile      { get; set; }
        
        /* Interface Implementation */
        public IPackageExtractor Extractor { get; set; } = new ArchiveExtractor();

        public bool IsCompatible(PathGenericTuple<ModConfig> mod)
        {
            var modDirectory = Path.GetDirectoryName(mod.Path);
            ConfigPath = GameBananaConfig.GetFilePath(modDirectory);
            return File.Exists(ConfigPath);
        }

        public void Construct(PathGenericTuple<ModConfig> mod)
        {
            Mod = mod;
            Config = GameBananaConfig.FromPath(ConfigPath);
        }

        public Version GetCurrentVersion()
        {
            // Autogenerate version from date.
            var fileTimeStamp = File.GetLastWriteTimeUtc(Mod.Path);
            return FromDateTime(fileTimeStamp);
        }

        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync()
        {
            try
            {
                Item = await GameBananaItem.FromTypeAndIdAsync(Config.ItemType, Config.ItemId);

                if (Item.Files.Values.Count > 0)
                {
                    ItemFile = Item.Files.First(x => x.Value.FileName.Contains(Config.FileNamePattern)).Value;
                    var date = ItemFile.DateAdded;
                    return new []{ FromDateTime(date) };
                }
            }
            catch (Exception) { /* Ignored */ }

            return new List<Version>();
        }

        public long GetSize()
        {
            return ItemFile.Filesize;
        }

        public async Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (sender, args) => { progress.Report(args.BytesReceived / (float)args.TotalBytesToReceive); };
                client.DownloadDataCompleted += (sender, args) => { progress.Report(1F); };
                var data = await client.DownloadDataTaskAsync(ItemFile.DownloadUrl);
                File.WriteAllBytes(destFilePath, data);
            }
        }

        public void PostUpdateCallback(bool hasUpdates) { }

        private Version FromDateTime(DateTime time)
        {
            return Version.Parse($"{time.Year}.{time.Month}.{time.Day}.{(int)time.TimeOfDay.TotalSeconds}");
        }

        public class GameBananaConfig : JsonSerializable<GameBananaConfig>
        {
            public const string ConfigFileName = "ReloadedGamebananaUpdater.json";
            public static string GetFilePath(string directoryFullPath) => $"{directoryFullPath}\\{ConfigFileName}";

            public string FileNamePattern { get; set; } = ".ReloadedII";
            public string ItemType { get; set; }
            public long ItemId     { get; set; }
        }
    }
}
